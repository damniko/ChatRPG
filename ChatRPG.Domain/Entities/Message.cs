using ChatRPG.Domain.Entities.Abstractions;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Domain.Entities;

public class Message : IEntity
{
    private Message()
    {
    }

    public Message(Campaign campaign, MessageRole role, string content, Verdict? verdict = null)
    {
        Campaign = campaign;
        Role = role;
        Content = content;
        Timestamp = DateTime.UtcNow;
        Verdict = verdict;
    }

    public int Id { get; init; }
    public Campaign Campaign { get; private set; } = null!;
    public MessageRole Role { get; private set; } = MessageRole.User;
    public string Content { get; private set; } = null!;
    public DateTime Timestamp { get; private set; }
    public Verdict? Verdict { get; private set; }
}
