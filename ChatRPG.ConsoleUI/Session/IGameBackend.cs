using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Gameplay;

namespace ChatRPG.ConsoleUI.Session;

/// <summary>
/// Everything the console needs from the game, with DI-scope management hidden behind it.
/// The live implementation opens one scope per call; the demo implementation needs neither a
/// database nor a language model.
/// </summary>
public interface IGameBackend
{
    Task<IReadOnlyList<CampaignSummary>> ListCampaignsAsync(CancellationToken ct = default);

    Task<int> CreateCampaignAsync(NewCampaignRequest request, CancellationToken ct = default);

    Task DeleteCampaignAsync(int campaignId, CancellationToken ct = default);

    Task<GameSnapshot> GetSnapshotAsync(int campaignId, CancellationToken ct = default);

    Task<IReadOnlyList<MessageView>> GetHistoryAsync(int campaignId, int count, CancellationToken ct = default);

    IAsyncEnumerable<TurnEvent> PlayTurnAsync(int campaignId, PlayerAction action, CancellationToken ct = default);

    IAsyncEnumerable<TurnEvent> StartCampaignAsync(int campaignId, string openingPrompt, CancellationToken ct = default);
}
