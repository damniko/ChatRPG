using System.Text;
using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Agents.Llm;

/// <summary>
/// Renders the <c>{gameSummary}</c>: the running summary of the campaign, optionally followed by the last few messages
/// verbatim so the model can see the exact wording of what just happened rather than only its summarised form.
/// </summary>
internal static class GameSummaryFormatter
{
    private const int IncludedPreviousMessages = 4;

    public static string Format(Campaign campaign, bool includePreviousMessages)
    {
        var summary = new StringBuilder()
            .Append("\n\nThe story up until now: ")
            .Append(campaign.GameSummary);

        if (!includePreviousMessages)
        {
            return summary.ToString();
        }

        summary.Append(
            "\n\nUse these previous messages as context. They only serve to give a hint of the current scenario:");

        foreach (var message in campaign.Messages.TakeLast(IncludedPreviousMessages))
        {
            if (message.Role == MessageRole.User)
            {
                summary.Append("\nPlayer: ").Append(message.Content);

                if (message.Verdict is { } verdict)
                {
                    summary.Append("\nAdherence verdict: ").Append(verdict.Content);
                }
            }
            else
            {
                summary.Append("\nGM: ").Append(message.Content).Append('\n');
            }
        }

        return summary.ToString();
    }
}
