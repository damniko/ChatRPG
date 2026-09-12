using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Configuration;
using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;
using Microsoft.Extensions.Options;
using Environment = ChatRPG.Domain.Entities.Environment;

namespace ChatRPG.Application.Campaigns;

public sealed class CreateCampaignHandler(
    IOptions<ApplicationOptions> options,
    ICampaignRepository campaigns,
    IScenarioDocumentStore documents,
    INarrativeGraphScribe graphScribe,
    IStartingScenarioGenerator scenarioGenerator,
    INarrativeGraphVisualizer graphVisualizer,
    IPortraitGenerator portraitGenerator,
    IUnitOfWork unitOfWork)
{
    public async Task<Campaign> HandleAsync(User user, string title, string startScenario, bool isOpenWorld, string characterName, string characterDescription, byte[]? scenarioDocument = null, CancellationToken ct = default)
    {
        var campaign = new Campaign(user, title, startScenario, isOpenWorld);
        var environment = new Environment(campaign, "Start location", "The place where it all began");
        var player = new Character(campaign, environment, CharacterType.Humanoid, characterName, characterDescription, true);
        campaign.Environments.Add(environment);
        campaign.Characters.Add(player);
        campaigns.Add(campaign);
        await unitOfWork.SaveChangesAsync(ct);

        if (!isOpenWorld)
            return campaign;
        
        ArgumentNullException.ThrowIfNull(scenarioDocument);
        await documents.IngestAsync(campaign.Id, scenarioDocument, ct);

        var progress = new Progress<int>(val =>
        {
            // TODO
        });
        var graph = await graphScribe.ScribeAsync(scenarioDocument, progress, ct);
        campaign.NarrativeGraph = graph;
        campaign.StartScenario = await scenarioGenerator.GenerateAsync(campaign, ct);

        if (options.Value.EnablePortraitGeneration)
        {
            byte[] portrait = await portraitGenerator.GenerateAsync(campaign.Player, campaign.StartScenario, ct);
            campaign.Player.Portrait = portrait;
        }
        await unitOfWork.SaveChangesAsync(ct);

        if (options.Value.EnableGraphVisualization)
        {
            graphVisualizer.Visualize(campaign.NarrativeGraph);
        }
        return campaign;
    }
}
