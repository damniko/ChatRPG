using System.Text;
using ChatRPG.Application.Abstractions;

namespace ChatRPG.Agents.Llm;

/// <summary>
/// Renders a window of messages as the verbatim transcript the models are shown.
/// </summary>
internal static class MessageTranscript
{
    public static void Append(StringBuilder builder, IEnumerable<MessageView> messages)
    {
        foreach (var message in messages)
        {
            switch (message)
            {
                case PlayerMessageView player:
                    builder.Append("\nPlayer: ").Append(player.Content);
                    if (player.Ruling is { } ruling)
                    {
                        builder.Append('\n').Append(ActionRulingFormatter.Format(ruling));
                    }

                    break;
                case NarrationMessageView narration:
                    builder.Append("\nGM: ").Append(narration.Content).Append('\n');
                    break;
            }
        }
    }
}
