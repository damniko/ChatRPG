using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatRPG.Infrastructure.Persistence;

public class CampaignRepository(ChatRpgDbContext dbContext) : ICampaignRepository
{
    public async Task<Campaign?> GetForPlayAsync(int campaignId, CancellationToken ct = default)
    {
        var campaign = await dbContext.Campaigns
            .Where(campaign => campaign.Id == campaignId)
            .Include(campaign => campaign.Messages)
            .Include(campaign => campaign.Locations)
            .Include(campaign => campaign.Characters)
            .Include(campaign => campaign.NarrativeGraph)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (campaign?.NarrativeGraph != null)
        {
            await dbContext.Entry(campaign.NarrativeGraph)
                .Collection(graph => graph.Nodes)
                .Query()
                .Include(node => node.Edges)
                .LoadAsync(cancellationToken: ct);
        }

        return campaign;
    }

    public async Task<IReadOnlyList<Campaign>> ListForUserAsync(int userId, CancellationToken ct = default)
    {
        return await dbContext.Campaigns
            .Where(campaign => campaign.User.Id.Equals(userId))
            .Include(campaign => campaign.Characters.Where(c => c.IsPlayer))
            .AsSplitQuery()
            .ToListAsync(cancellationToken: ct);
    }

    public void Add(Campaign campaign)
    {
        dbContext.Campaigns.Add(campaign);
    }

    public void Remove(Campaign campaign)
    {
        dbContext.Campaigns.Remove(campaign);
    }
}
