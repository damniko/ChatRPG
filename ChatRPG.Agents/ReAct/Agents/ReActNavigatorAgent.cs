using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.ReAct.Agents;

internal sealed class ReActNavigatorAgent(
    IChatModelFactory models,
    GameSummaryFormatter summaryFormatter,
    IInstructionCatalog instructions,
    IToolFactory toolFactory) : IGraphNavigator
{
    private const double Temperature = 0.4;
    
    public async Task<string> ReviewGraphAsync(
        Campaign campaign,
        string playerInput,
        ActionRuling ruling,
        CancellationToken ct = default)
    {
        string gameSummary = await summaryFormatter.FormatAsync(campaign.Id, campaign.GameSummary, ct);

        // TODO: Improve NarrativeGraph detection ("open world" vs scenario-based)
        var agent = new ReActAgent(models.CreateChat(Temperature, nameof(ReActNavigatorAgent)), instructions.Get(InstructionKey.Navigate))
        {
            Variables =
            {
                ["graph"] = campaign.NarrativeGraph is { } g ? NarrativeGraphFormatter.Format(g) : string.Empty,
                ["gameSummary"] = gameSummary
            },
            Tools =
            {
                toolFactory.GetUpdateGraphTool(campaign, gameSummary, ruling)
            }
        };

        string formattedInput = $"Player input: {playerInput}\n{ActionRulingFormatter.Format(ruling)}";
        return await agent.RunAsync(formattedInput, ct);
    }
}
