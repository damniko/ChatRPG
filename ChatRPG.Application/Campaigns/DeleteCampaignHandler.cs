using ChatRPG.Application.Abstractions;

namespace ChatRPG.Application.Campaigns;

public sealed class DeleteCampaignHandler(
    ICampaignRepository campaigns,
    IScenarioDocumentStore documents,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int campaignId, CancellationToken ct = default)
    {
        var campaign = await campaigns.GetForPlayAsync(campaignId, ct);
        if (campaign == null) return;

        campaigns.Remove(campaign);
        await unitOfWork.SaveChangesAsync(ct);

        await documents.DeleteAsync(campaign.Id, ct);
    }
}
