using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Gameplay;

public interface IGameTurnService
{
    IAsyncEnumerable<TurnEvent> PlayTurnAsync(Campaign campaign, PlayerAction action, CancellationToken ct = default);
    IAsyncEnumerable<TurnEvent> StartCampaignAsync(Campaign campaign, string openingPrompt, CancellationToken ct = default);
}
