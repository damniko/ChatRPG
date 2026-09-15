using System.Text;
using ChatRPG.Agents.Configuration;
using ChatRPG.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.Llm;

/// <summary>
/// Renders the <c>{gameSummary}</c>: the running summary of the campaign, optionally followed by the last few messages
/// verbatim so the model can see the exact wording of what just happened rather than only its summarised form.
/// </summary>
internal sealed class GameSummaryFormatter(IMessageHistory messages, IOptions<AgentOptions> options)
{
    /// <summary>Number of messages to include when summarizing from message history.</summary>
    private const int MessageWindowSize = 4;

    public async Task<string> FormatAsync(int campaignId, string currentSummary, CancellationToken ct = default)
    {
        var summary = new StringBuilder()
            .Append("\n\nThe story up until now: ")
            .Append(currentSummary);

        if (!options.Value.IncludePreviousMessages)
        {
            return summary.ToString();
        }

        summary.Append(
            "\n\nUse these previous messages as context. They only serve to give a hint of the current scenario:");

        var messageWindow = await messages.GetRecentAsync(campaignId, MessageWindowSize, ct);
        MessageTranscript.Append(summary, messageWindow);

        return summary.ToString();
    }
}
