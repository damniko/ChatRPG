using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.ReAct;

internal sealed class ReActNavigatorAgent(
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IOptions<AgentOptions> options,
    IToolFactory toolFactory) : IGraphNavigator
{
    private const double Temperature = 0.4;
    
    public async Task<string> ReviewGraphAsync(
        Campaign campaign,
        string playerInput,
        AdherenceVerdict verdict,
        CancellationToken ct = default)
    {
        string gameSummary = GameSummaryFormatter.Format(campaign, options.Value.IncludePreviousMessages);

        // TODO: Improve NarrativeGraph detection ("open world" vs scenario-based)
        var agent = new ReActAgent(models.CreateChat(Temperature, nameof(ReActNavigatorAgent)), instructions.Get(InstructionKey.Navigate))
        {
            Variables =
            {
                ["graph"] = campaign.NarrativeGraph?.Serialize() ?? string.Empty,
                ["gameSummary"] = gameSummary
            },
            Tools =
            {
                toolFactory.GetUpdateGraphTool(campaign, verdict)
            }
        };

        // TODO: Consider a helper for formatting the input
        string formattedInput = $"Player input: {playerInput}\nAdherence verdict: {verdict.ToPromptString()}";
        return await agent.RunAsync(formattedInput, ct);
    }
}
