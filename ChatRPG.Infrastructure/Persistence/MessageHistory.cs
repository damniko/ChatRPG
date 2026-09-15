using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Mapping;
using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatRPG.Infrastructure.Persistence;

internal sealed class MessageHistory(ChatRpgDbContext db) : IMessageHistory
{
    public async Task<IReadOnlyList<MessageView>> GetRecentAsync(int campaignId, int count, CancellationToken ct = default)
    {
        var list = await db.Set<Message>()
            .Where(m => m.Campaign.Id == campaignId)
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .ToListAsync(ct);
        return [.. list.OrderBy(m => m.Timestamp).Select(m => m.ToView())];
    }
}