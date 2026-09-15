using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public abstract class Message : IEntity
{
    protected Message() { }

    protected Message(Campaign campaign, string content)
    {
        Campaign = campaign;
        Content = content;
        Timestamp = DateTime.UtcNow;
    }

    public int Id { get; init; }
    public required Campaign Campaign { get; init; }
    public required string Content { get; init; }
    public DateTime Timestamp { get; init; }
}
