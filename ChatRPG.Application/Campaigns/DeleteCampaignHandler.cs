using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Campaigns;

public sealed class DeleteCampaignHandler(
    ICampaignRepository campaigns,
    IScenarioDocumentStore documents,
    IVisualizationStore visualizations,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int campaignId, CancellationToken ct = default)
    {
        var campaign = await campaigns.GetForPlayAsync(campaignId, ct);
        if (campaign == null) return;

        campaigns.Remove(campaign);
        await DeleteVisualizationsAsync(campaign, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await documents.DeleteAsync(campaign.Id, ct);
    }

    private async Task DeleteVisualizationsAsync(Campaign campaign, CancellationToken ct = default)
    {
        if (campaign.NarrativeGraph is null)
            return;

        foreach (var visualization in campaign.NarrativeGraph.Visualizations)
            await visualizations.DeleteAsync(visualization.Location, ct);
    }
}
