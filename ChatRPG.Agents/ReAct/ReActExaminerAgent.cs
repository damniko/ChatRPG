using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.ReAct;

/// <summary>
/// Decides whether the player is allowed to do as they say, before it is narrated.
/// The verdict is grounded in the narrative graph and the scenario document.
/// </summary>
internal sealed class ReActExaminerAgent(
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IToolFactory toolFactory,
    IOptions<AgentOptions> options) : IInputExaminer
{
    private const double Temperature = 0.4;
    private const string VerdictMarker = "Verdict:";

    public async Task<AdherenceVerdict> ExamineAsync(
        Campaign campaign,
        string playerInput,
        CancellationToken ct = default)
    {
        var graph = campaign.NarrativeGraph ?? throw new InvalidOperationException(
            "Only campaigns with a narrative graph are examined.");

        string gameSummary = GameSummaryFormatter.Format(campaign, options.Value.IncludePreviousMessages);

        var agent = new ReActAgent(models.CreateChat(Temperature, nameof(ReActExaminerAgent)), instructions.Get(InstructionKey.Examine))
        {
            Variables =
            {
                ["graph"] = NarrativeGraphFormatter.Format(graph),
                ["gameSummary"] = gameSummary,
            },
            Tools =
            {
                toolFactory.GetSearchScenarioTool(campaign)
            }
        };

        return ParseVerdict(await agent.RunAsync(playerInput, ct));
    }

    /// <summary>
    /// Splits the answer the prompt asks for - "Verdict: &lt;ALLOWED | CONDITIONALLY ALLOWED |
    /// DISALLOWED&gt;" followed by the reasoning - into its two halves. Conditionally allowed counts
    /// as allowed: the player may try, and the reasoning tells the narrator what stands in the way.
    /// </summary>
    internal static AdherenceVerdict ParseVerdict(string answer)
    {
        bool isAllowed = !answer.Contains("DISALLOWED", StringComparison.OrdinalIgnoreCase);

        // Keep the whole answer as the reasoning if the model ran the two together on one line.
        int verdict = answer.IndexOf(VerdictMarker, StringComparison.OrdinalIgnoreCase);
        int endOfVerdict = verdict < 0 ? -1 : answer.IndexOf('\n', verdict);
        string reasoning = endOfVerdict < 0 ? string.Empty : answer[(endOfVerdict + 1)..].Trim();

        return new AdherenceVerdict(isAllowed, reasoning.Length > 0 ? reasoning : answer.Trim());
    }
}
