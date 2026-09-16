using System.Runtime.CompilerServices;
using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Campaigns;
using ChatRPG.Application.Gameplay;
using ChatRPG.ConsoleUI.Configuration;
using ChatRPG.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRPG.ConsoleUI.Session;

public sealed class LiveGameBackend(IServiceScopeFactory scopeFactory, PlayerSettings settings) : IGameBackend
{
    public async Task<IReadOnlyList<CampaignSummary>> ListCampaignsAsync(CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        User user = await GetUserAsync(scope, ct);
        var campaigns = scope.ServiceProvider.GetRequiredService<ICampaignRepository>();

        IReadOnlyList<Campaign> found = await campaigns.ListForUserAsync(user.Id, ct);
        return found
            .Select(c => new CampaignSummary(
                c.Id,
                c.Title,
                c.Characters.FirstOrDefault(ch => ch.IsPlayer)?.Name ?? "?",
                c.StartedOn,
                c.GameOver))
            .OrderByDescending(c => c.StartedOn)
            .ToList();
    }

    public async Task<int> CreateCampaignAsync(NewCampaignRequest request, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        User user = await GetUserAsync(scope, ct);
        var handler = scope.ServiceProvider.GetRequiredService<CreateCampaignHandler>();

        Campaign campaign = await handler.HandleAsync(
            user,
            request.Title,
            request.StartScenario,
            request.IsOpenWorld,
            request.CharacterName,
            request.CharacterDescription,
            request.ScenarioDocument,
            ct);

        return campaign.Id;
    }

    public async Task DeleteCampaignAsync(int campaignId, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<DeleteCampaignHandler>();
        await handler.HandleAsync(campaignId, ct);
    }

    public async Task<GameSnapshot> GetSnapshotAsync(int campaignId, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        Campaign campaign = await LoadAsync(scope, campaignId, ct);
        return Project(campaign);
    }

    public async Task<IReadOnlyList<MessageView>> GetHistoryAsync(int campaignId, int count, CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        var history = scope.ServiceProvider.GetRequiredService<IMessageHistory>();
        return await history.GetRecentAsync(campaignId, count, ct);
    }

    public async IAsyncEnumerable<TurnEvent> PlayTurnAsync(
        int campaignId, PlayerAction action, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        Campaign campaign = await LoadAsync(scope, campaignId, ct);
        var turns = scope.ServiceProvider.GetRequiredService<IGameTurnService>();

        await foreach (TurnEvent turnEvent in turns.PlayTurnAsync(campaign, action, ct))
        {
            yield return turnEvent;
        }
    }

    public async IAsyncEnumerable<TurnEvent> StartCampaignAsync(
        int campaignId, string openingPrompt, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        Campaign campaign = await LoadAsync(scope, campaignId, ct);
        var turns = scope.ServiceProvider.GetRequiredService<IGameTurnService>();

        await foreach (TurnEvent turnEvent in turns.StartCampaignAsync(campaign, openingPrompt, ct))
        {
            yield return turnEvent;
        }
    }

    private Task<User> GetUserAsync(AsyncServiceScope scope, CancellationToken ct)
    {
        var directory = scope.ServiceProvider.GetRequiredService<IUserDirectory>();
        return directory.GetOrCreateAsync(settings.IdentityId, settings.PlayerName, ct);
    }

    private static async Task<Campaign> LoadAsync(AsyncServiceScope scope, int campaignId, CancellationToken ct)
    {
        var campaigns = scope.ServiceProvider.GetRequiredService<ICampaignRepository>();
        return await campaigns.GetForPlayAsync(campaignId, ct)
               ?? throw new InvalidOperationException($"Campaign {campaignId} was not found.");
    }

    private static GameSnapshot Project(Campaign campaign)
    {
        Character player = campaign.Player;
        string playerLocation = LocationOf(player);

        List<PartyMember> party = campaign.Characters
            .Where(c => c.IsPlayer || (c.CurrentHealth > 0 && LocationOf(c) == playerLocation))
            .OrderByDescending(c => c.IsPlayer)
            .ThenBy(c => c.Name)
            .Select(c => new PartyMember(c.Name, c.CurrentHealth, c.MaxHealth, LocationOf(c), c.IsPlayer))
            .ToList();

        return new GameSnapshot(
            campaign.Id,
            campaign.Title,
            campaign.IsOpenWorld,
            campaign.GameOver,
            player.Name,
            playerLocation,
            party,
            campaign.Characters.Select(c => c.Name).Distinct().ToList(),
            campaign.Locations.Select(l => l.Name).Distinct().ToList());
    }

    private static string LocationOf(Character character) => character.Location?.Name ?? "somewhere";
}
