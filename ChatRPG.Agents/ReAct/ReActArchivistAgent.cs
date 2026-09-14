using System.Text.Json;
using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using LangChain.Memory;
using LangChain.Providers;
using Microsoft.Extensions.Options;
using Message = ChatRPG.Domain.Entities.Message;

namespace ChatRPG.Agents.ReAct;

internal sealed class ReActArchivistAgent(
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IToolFactory toolFactory,
    IOptions<AgentOptions> options) : IArchivist
{
    private const double NarrativeChangesTemperature = 0.7;
    private const double SummaryTemperature = 0.4;

    /// <summary>Labels the summary separately, since it is a second call with its own cost.</summary>
    private const string SummaryOperation = $"{nameof(ReActArchivistAgent)}.Summary";

    public async Task ApplyNarrativeChangesAsync(
        Campaign campaign,
        string playerInput,
        string narration,
        CancellationToken ct = default)
    {
        string characters = JsonSerializer.Serialize(campaign.Characters.Select(c => new { c.Name, c.Description, c.Type }));
        string environments = JsonSerializer.Serialize(campaign.Environments.Select(e => new { e.Name, e.Description }));
        string gameSummary = GameSummaryFormatter.Format(campaign, options.Value.IncludePreviousMessages);

        var agent = new ReActAgent(models.CreateChat(NarrativeChangesTemperature, nameof(ReActArchivistAgent)), instructions.Get(InstructionKey.Archive))
        {
            Variables =
            {
                ["characters"] = characters,
                ["environments"] = environments,
                ["player_character"] = campaign.Player.Name,
                ["gameSummary"] = gameSummary
            },
            Tools =
            {
                toolFactory.GetUpdateCharacterTool(campaign),
                toolFactory.GetUpdateEnvironmentTool(campaign)
            }
        };

        // TODO: Format in a helper or similar
        string modelInput = $"The player says: {playerInput}\nThe DM says: {narration}";
        await agent.RunAsync(modelInput, ct);
    }

    public async Task AppendMessagesAsync(
        Campaign campaign,
        string playerInput,
        string narration,
        AdherenceVerdict? adherenceVerdict,
        string? epilogue,
        CancellationToken ct = default)
    {
        if (options.Value.SummarizeArchivistMessages)
        {
            var summaryModel = models.CreateChat(SummaryTemperature, SummaryOperation);
            var messages = new List<LangChain.Providers.Message>
            {
                new(playerInput, MessageRole.Human),
                new(narration, MessageRole.Ai)
            };
            string gameSummary = await summaryModel.SummarizeAsync(messages, campaign.GameSummary, cancellationToken: ct);
            campaign.GameSummary = gameSummary;
        }
        else
        {
            campaign.GameSummary += $"Player: {playerInput}\n";
            if (adherenceVerdict is not null)
            {
                campaign.GameSummary += $"Scenario Adherence Verdict: {adherenceVerdict.ToPromptString()}";
            }
            campaign.GameSummary += $"GM: {narration}\n\n";
        }

        Verdict? verdict = null;
        if (adherenceVerdict is not null)
        {
            verdict = new Verdict(campaign, adherenceVerdict.ToPromptString());
        }

        if (campaign.Messages.Count != 0)
        {
            campaign.Messages.Add(new Message(campaign, Domain.Enums.MessageRole.User, playerInput, verdict));
        }
        
        campaign.Messages.Add(new Message(campaign, Domain.Enums.MessageRole.Assistant, narration));

        if (epilogue != null)
        {
            campaign.Messages.Add(new Message(campaign, Domain.Enums.MessageRole.Assistant, epilogue));
        }
    }
}
