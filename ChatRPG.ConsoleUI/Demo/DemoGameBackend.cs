using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Gameplay;
using ChatRPG.ConsoleUI.Session;
using ChatRPG.Domain.Entities;

namespace ChatRPG.ConsoleUI.Demo;

/// <summary>
/// Replays a canned turn so the console can be exercised without a database or a language model.
/// Enabled with <c>--demo</c>; it is the loop to iterate on when changing rendering.
/// </summary>
public sealed class DemoGameBackend : IGameBackend
{
    private const int CampaignId = 1;

    private static readonly string[] Narration =
    [
        "The door groans open on hinges that have not turned in a hundred years. ",
        "Dust rolls off the lintel and settles across the flagstones in a slow grey tide. ",
        "Beyond the threshold the passage falls away into a dark that your torchlight refuses to enter, ",
        "and from somewhere far below comes a sound like a held breath finally released.\n\n",
        "Bram sets his shoulder against the frame and looks at you. He does not say anything. ",
        "He does not have to."
    ];

    private readonly List<PartyMember> _party =
    [
        new("Kira", 14, 20, "Crypt Entrance", IsPlayer: true),
        new("Bram", 20, 20, "Crypt Entrance", IsPlayer: false)
    ];

    private int _turn;

    public Task<IReadOnlyList<CampaignSummary>> ListCampaignsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<CampaignSummary>>(
            [new CampaignSummary(CampaignId, "The Hollow Crypt (demo)", "Kira", new DateTime(2026, 1, 1), false)]);

    public Task<int> CreateCampaignAsync(NewCampaignRequest request, CancellationToken ct = default) =>
        Task.FromResult(CampaignId);

    public Task DeleteCampaignAsync(int campaignId, CancellationToken ct = default) => Task.CompletedTask;

    public Task<GameSnapshot> GetSnapshotAsync(int campaignId, CancellationToken ct = default) =>
        Task.FromResult(new GameSnapshot(
            CampaignId,
            "The Hollow Crypt (demo)",
            IsOpenWorld: false,
            GameOver: false,
            "Kira",
            "Crypt Entrance",
            [.. _party],
            _party.Select(m => m.Name).ToList(),
            ["Crypt Entrance", "The Long Stair", "Ossuary"]));

    public Task<IReadOnlyList<MessageView>> GetHistoryAsync(int campaignId, int count, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<MessageView>>(
        [
            new PlayerMessageView(new DateTime(2026, 1, 1, 12, 0, 0), "I check the door for traps.", null),
            new NarrationMessageView(new DateTime(2026, 1, 1, 12, 0, 5),
                "You find nothing but rust and the long habit of neglect. Whatever guarded this place stopped caring a long time ago.")
        ]);

    public IAsyncEnumerable<TurnEvent> StartCampaignAsync(
        int campaignId, string openingPrompt, CancellationToken ct = default) =>
        Replay(ruling: null, ct);

    public IAsyncEnumerable<TurnEvent> PlayTurnAsync(
        int campaignId, PlayerAction action, CancellationToken ct = default)
    {
        // Cycle the permissions so every rendering path is reachable from the demo.
        ActionPermission permission = (_turn++ % 3) switch
        {
            0 => ActionPermission.Allowed,
            1 => ActionPermission.Conditional,
            _ => ActionPermission.Disallowed
        };

        return Replay(new ActionRuling(permission, "The hinges are rusted, but the frame has already given way."), ct);
    }

    private async IAsyncEnumerable<TurnEvent> Replay(
        ActionRuling? ruling,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        if (ruling is not null)
        {
            await Task.Delay(700, ct);
            yield return new TurnEvent.RulingIssued(ruling);

            if (!ruling.AttemptProceeds)
            {
                yield return new TurnEvent.InputRejected(ruling.Reasoning);
            }
        }

        await Task.Delay(900, ct);
        yield return new TurnEvent.NarrationStarted();

        foreach (string sentence in Narration)
        {
            // Split further so the writer sees the same small chunks a model would produce.
            foreach (string chunk in Chunk(sentence, 7))
            {
                await Task.Delay(60, ct);
                yield return new TurnEvent.NarrationChunk(chunk);
            }
        }

        yield return new TurnEvent.NarrationCompleted(string.Concat(Narration));

        yield return new TurnEvent.ArchivingStarted();
        await Task.Delay(1500, ct);

        // Wound the player so the snapshot diff has something to report.
        _party[0] = _party[0] with { CurrentHealth = Math.Max(0, _party[0].CurrentHealth - 3) };

        yield return new TurnEvent.CampaignSaved();
    }

    private static IEnumerable<string> Chunk(string text, int size)
    {
        for (int i = 0; i < text.Length; i += size)
        {
            yield return text.Substring(i, Math.Min(size, text.Length - i));
        }
    }
}
