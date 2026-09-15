using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Mapping;

public static class MessageMappingExtensions
{
    public static MessageView ToView(this Message message)
    {
        return message switch
        {
            PlayerMessage player => new PlayerMessageView(player.Timestamp, player.Content, player.Ruling),
            NarrationMessage narration => new NarrationMessageView(narration.Timestamp, narration.Content),
            _ => throw new ArgumentOutOfRangeException(nameof(message), message.GetType().Name, "Unknown message type")
        };
    }
}
