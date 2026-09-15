using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.ReAct;

/// <summary>
/// Decides whether the player is allowed to do as they say, before it is narrated.
/// The ruling is grounded in the narrative graph and the scenario document.
/// </summary>
internal sealed class ReActExaminerAgent(
    IChatModelFactory models,
    GameSummaryFormatter summaryFormatter,
    IInstructionCatalog instructions,
    IToolFactory toolFactory) : IActionExaminer
{
    private const double Temperature = 0.4;
    private const string RulingMarker = "Ruling:";

    /// <summary>Stands in for reasoning when the model returns nothing usable.</summary>
    private const string MissingReasoning = "The examiner gave no reasoning.";

    public async Task<ActionRuling> ExamineAsync(
        Campaign campaign,
        string playerInput,
        CancellationToken ct = default)
    {
        var graph = campaign.NarrativeGraph ?? throw new InvalidOperationException(
            "Only campaigns with a narrative graph are examined.");

        string gameSummary = await summaryFormatter.FormatAsync(campaign.Id, campaign.GameSummary, ct);

        var agent = new ReActAgent(models.CreateChat(Temperature, nameof(ReActExaminerAgent)), instructions.Get(InstructionKey.Examine))
        {
            Variables =
            {
                ["graph"] = NarrativeGraphFormatter.Format(graph),
                ["gameSummary"] = gameSummary,
            },
            Tools =
            {
                toolFactory.GetSearchScenarioTool(campaign, gameSummary)
            }
        };

        return ParseRuling(await agent.RunAsync(playerInput, ct));
    }

    /// <summary>
    /// Splits the answer the prompt asks for - "Ruling: &lt;ALLOWED | CONDITIONAL | DISALLOWED&gt;"
    /// followed by the reasoning - into its two halves. The permission is read from the ruling line
    /// alone, so reasoning that happens to mention "disallowed" cannot flip it. An answer that does
    /// not follow the format falls back to allowed with the whole answer as the reasoning, rather
    /// than blocking the player over a formatting slip.
    /// </summary>
    internal static ActionRuling ParseRuling(string answer)
    {
        int marker = answer.IndexOf(RulingMarker, StringComparison.OrdinalIgnoreCase);
        if (marker < 0)
        {
            return new ActionRuling(ActionPermission.Allowed, Reasoning(answer, MissingReasoning));
        }

        int afterMarker = marker + RulingMarker.Length;
        int endOfLine = answer.IndexOf('\n', afterMarker);
        string rulingLine = endOfLine < 0 ? answer[afterMarker..] : answer[afterMarker..endOfLine];
        string rest = endOfLine < 0 ? string.Empty : answer[(endOfLine + 1)..];

        var permission = ReadPermission(rulingLine, out int endOfToken);

        // The model may have run the reasoning onto the ruling line instead of the next one.
        string reasoning = rest.Trim().Length > 0
            ? rest
            : rulingLine[endOfToken..].TrimStart(' ', '\t', '-', ':', '–', '—');

        return new ActionRuling(permission, Reasoning(reasoning, MissingReasoning));
    }

    /// <summary>
    /// Reads the permission token from the ruling line. DISALLOWED is matched before ALLOWED, which
    /// it contains, and CONDITIONAL before either, as the model may write "CONDITIONALLY ALLOWED".
    /// </summary>
    private static ActionPermission ReadPermission(string rulingLine, out int endOfToken)
    {
        foreach ((string token, var permission) in (ReadOnlySpan<(string, ActionPermission)>)
                 [
                     ("CONDITIONAL", ActionPermission.Conditional),
                     ("DISALLOWED", ActionPermission.Disallowed),
                     ("ALLOWED", ActionPermission.Allowed)
                 ])
        {
            int at = rulingLine.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (at < 0)
            {
                continue;
            }

            endOfToken = at + token.Length;

            // "CONDITIONALLY ALLOWED" is one token, so skip past the ALLOWED that follows it.
            if (permission is ActionPermission.Conditional)
            {
                int allowed = rulingLine.IndexOf("ALLOWED", endOfToken, StringComparison.OrdinalIgnoreCase);
                if (allowed >= 0)
                {
                    endOfToken = allowed + "ALLOWED".Length;
                }
            }

            return permission;
        }

        endOfToken = 0;
        return ActionPermission.Allowed;
    }

    private static string Reasoning(string candidate, string fallback)
    {
        string trimmed = candidate.Trim();
        return trimmed.Length > 0 ? trimmed : fallback;
    }
}
