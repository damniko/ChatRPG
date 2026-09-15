using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IMessageHistory
{
    Task<IReadOnlyList<MessageView>> GetRecentAsync(int campaignId, int count, CancellationToken ct = default);
}

public abstract record MessageView(DateTime Timestamp, string Content);

public sealed record PlayerMessageView(DateTime Timestamp, string Content, ActionRuling? Ruling)
    : MessageView(Timestamp, Content);

public sealed record NarrationMessageView(DateTime Timestamp, string Content)
    : MessageView(Timestamp, Content);
