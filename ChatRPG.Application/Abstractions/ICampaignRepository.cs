using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface ICampaignRepository
{
    Task<Campaign?> GetForPlayAsync(int campaignId, CancellationToken ct = default);
    Task<IReadOnlyList<Campaign>> ListForUserAsync(int userId, CancellationToken ct = default);
    void Add(Campaign campaign);
    void Remove(Campaign campaign);
}
