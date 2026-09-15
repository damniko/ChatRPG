using System.Text;
using ChatRPG.Agents.Llm;
using ChatRPG.Application.Abstractions;

namespace ChatRPG.Agents.Summarization;

/// <summary>
/// Renders a single exchange that has not been stored as messages yet. The counterpart to
/// <see cref="MessageTranscript" />, which renders a window of already stored messages.
/// </summary>
internal static class ExchangeTranscript
{
    public static string Format(SummaryRequest request)
    {
        var builder = new StringBuilder()
            .Append("Player: ").Append(request.PlayerInput).Append('\n');

        if (request.Ruling is { } ruling)
        {
            builder.Append(ActionRulingFormatter.Format(ruling)).Append('\n');
        }

        return builder.Append("GM: ").Append(request.Narration).Append("\n\n").ToString();
    }
}
