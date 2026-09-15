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
    GameSummaryFormatter summaryFormatter,
    IInstructionCatalog instructions,
    IToolFactory toolFactory,
    IOptions<AgentOptions> options) : IArchivist
{
    private const double NarrativeChangesTemperature = 0.7;
    private const double SummaryTemperature = 0.4;

    public async Task<ArchiveResult> ApplyNarrativeChangesAsync(ArchiveRequest request, CancellationToken ct = default)
    {
        string characters = JsonSerializer.Serialize(request.Characters.Select(c => new { c.Name, c.Description }));
        string locations = JsonSerializer.Serialize(request.Locations);
        string gameSummary = await summaryFormatter.FormatAsync(request.CampaignId, request.GameSummary, ct);

        var collector = new ChangeCollector();
        var agent = new ReActAgent(models.CreateChat(NarrativeChangesTemperature, nameof(ReActArchivistAgent)), instructions.Get(InstructionKey.Archive))
        {
            Variables =
            {
                ["characters"] = characters,
                ["locations"] = locations,
                ["player_character"] = request.Characters.First(c => c.IsPlayer).Name,
                ["gameSummary"] = gameSummary
            },
            Tools =
            {
                toolFactory.GetUpdateCharacterTool(request.Characters, collector),
                toolFactory.GetUpdateLocationTool(request.Locations, collector)
            }
        };

        // TODO: Format in a helper or similar
        string modelInput = $"The player says: {request.PlayerInput}\nThe DM says: {request.Narration}";
        await agent.RunAsync(modelInput, ct);

        return new ArchiveResult(
            "new summary", collector.CharacterChanges, collector.NewCharacters, collector.LocationChanges);
    }

    public async Task AppendMessagesAsync(
        Campaign campaign,
        string playerInput,
        string narration,
        ActionRuling? ruling,
        string? epilogue,
        CancellationToken ct = default)
    {
        if (options.Value.SummarizeArchivistMessages)
        {
            var summaryModel = models.CreateChat(SummaryTemperature);
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
            if (ruling is not null)
            {
                campaign.GameSummary += $"{ActionRulingFormatter.Format(ruling)}\n";
            }
            campaign.GameSummary += $"GM: {narration}\n\n";
        }

        if (campaign.Messages.Count != 0)
        {
            campaign.Messages.Add(new PlayerMessage(campaign, playerInput, ruling));
        }

        campaign.Messages.Add(new NarrationMessage(campaign, narration));

        if (epilogue != null)
        {
            campaign.Messages.Add(new NarrationMessage(campaign, epilogue));
        }
    }
}
