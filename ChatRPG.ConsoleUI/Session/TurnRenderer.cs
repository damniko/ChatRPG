using ChatRPG.Application.Gameplay;
using ChatRPG.ConsoleUI.Configuration;
using ChatRPG.ConsoleUI.Rendering;
using Spectre.Console;

namespace ChatRPG.ConsoleUI.Session;

public sealed record TurnOutcome(bool Cancelled, bool GameEnded, bool Saved);

/// <summary>
/// Drives one turn's event stream to the console.
/// </summary>
/// <remarks>
/// Spectre's status spinner owns the cursor for as long as its block is open and nothing else may
/// write inside it, so the stream is pumped by hand rather than with <c>await foreach</c>: waits
/// that produce no output happen under a spinner, and narration chunks are written outside it.
/// </remarks>
public sealed class TurnRenderer(PlayerSettings settings)
{
    public async Task<TurnOutcome> RenderAsync(IAsyncEnumerable<TurnEvent> events, CancellationToken ct)
    {
        int width = settings.NarrationWidth > 0
            ? settings.NarrationWidth
            : Math.Max(40, AnsiConsole.Profile.Width - 2);

        var narration = new NarrationWriter(width);
        bool gameEnded = false;
        bool saved = false;

        await using IAsyncEnumerator<TurnEvent> events1 = events.GetAsyncEnumerator(ct);

        try
        {
            // The examiner and navigator both run before the first event is yielded, so the very
            // first wait is the longest one in the turn.
            bool hasEvent = await Spin("consulting the examiner", () => events1.MoveNextAsync().AsTask(), ct);

            while (hasEvent)
            {
                TurnEvent turnEvent = events1.Current;

                if (turnEvent is TurnEvent.ArchivingStarted)
                {
                    narration.Flush();
                    AnsiConsole.WriteLine();

                    // Nothing is written between here and CampaignSaved, so the rest of the stream
                    // can be drained inside the spinner.
                    (gameEnded, saved) = await Spin("archiving the turn", async () =>
                    {
                        bool ended = false;
                        bool persisted = false;
                        while (await events1.MoveNextAsync())
                        {
                            if (events1.Current is TurnEvent.GameEnded) ended = true;
                            if (events1.Current is TurnEvent.CampaignSaved) persisted = true;
                        }
                        return (ended, persisted);
                    }, ct);

                    break;
                }

                bool narrating = Render(turnEvent, narration);

                if (turnEvent is TurnEvent.GameEnded) gameEnded = true;
                if (turnEvent is TurnEvent.CampaignSaved) saved = true;

                hasEvent = narrating
                    ? await events1.MoveNextAsync()
                    : await Spin("the narrator considers", () => events1.MoveNextAsync().AsTask(), ct);
            }

            narration.Flush();
            return new TurnOutcome(Cancelled: false, gameEnded, saved);
        }
        catch (OperationCanceledException)
        {
            narration.Flush();
            AnsiConsole.WriteLine();
            EventRenderer.Warn("turn cancelled");
            AnsiConsole.WriteLine();
            return new TurnOutcome(Cancelled: true, GameEnded: false, Saved: false);
        }
    }

    /// <summary>Renders one event and reports whether narration is currently streaming.</summary>
    private static bool Render(TurnEvent turnEvent, NarrationWriter narration)
    {
        switch (turnEvent)
        {
            case TurnEvent.RulingIssued ruling:
                EventRenderer.Ruling(ruling.Ruling);
                return false;

            case TurnEvent.InputRejected rejected:
                EventRenderer.InputRejected(rejected.Reasoning);
                return false;

            case TurnEvent.NarrationChunk chunk:
                narration.Write(chunk.Text);
                return true;

            case TurnEvent.NarrationStarted:
                return true;

            case TurnEvent.NarrationCompleted:
                narration.Flush();
                AnsiConsole.WriteLine();
                return false;

            case TurnEvent.GameEnded ended:
                EventRenderer.Epilogue(ended.Epilogue);
                return false;

            default:
                return false;
        }
    }

    private static Task<T> Spin<T>(string label, Func<Task<T>> work, CancellationToken ct)
    {
        // Status needs an interactive terminal; when output is redirected it would throw.
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            EventRenderer.Info($"{label}…");
            return work();
        }

        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Theme.Accent)
            .StartAsync($"[grey54]{label}…[/]", _ =>
            {
                ct.ThrowIfCancellationRequested();
                return work();
            });
    }
}
