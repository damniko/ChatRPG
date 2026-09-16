using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Gameplay;
using ChatRPG.ConsoleUI.Configuration;
using ChatRPG.ConsoleUI.Input;
using ChatRPG.ConsoleUI.Menus;
using ChatRPG.ConsoleUI.Rendering;
using PrettyPrompt;
using PrettyPrompt.Highlighting;
using Spectre.Console;

namespace ChatRPG.ConsoleUI.Session;

/// <summary>
/// The read-eval-print loop.
/// </summary>
/// <remarks>
/// PrettyPrompt owns the console only inside <c>ReadLineAsync</c>. Everything Spectre writes —
/// panels, rules, spinners, menus — happens strictly between reads, and nothing writes from a
/// background thread while a prompt is open.
/// </remarks>
public sealed class GameLoop(
    IGameBackend backend,
    MainMenu menu,
    SettingsEditor settingsEditor,
    PlayerSettingsStore settingsStore,
    PlayerSettings settings)
{
    private PlayerSettings _settings = settings;
    private GameSnapshot? _snapshot;

    public async Task RunAsync(CancellationToken ct)
    {
        Theme.Apply(_settings);
        var callbacks = new GamePromptCallbacks(_settings);
        await using var prompt = new Prompt(
            persistentHistoryFilepath: _settings.PersistHistory ? settingsStore.HistoryPath : null,
            callbacks: callbacks,
            configuration: new PromptConfiguration(prompt: new FormattedString("> ", new ConsoleFormat(AnsiColor.BrightMagenta))));

        WriteBanner();

        if (!await OpenMenuAsync(ct))
        {
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            if (_snapshot is null)
            {
                if (!await OpenMenuAsync(ct))
                {
                    return;
                }
                continue;
            }

            callbacks.Snapshot = _snapshot;

            PromptResult input = await prompt.ReadLineAsync();

            string text = input is KeyPressCallbackResult callback
                ? callback.Output ?? string.Empty
                : input.Text.Trim();

            if (input is not KeyPressCallbackResult && !input.IsSuccess)
            {
                // Ctrl-C while editing: drop the line and redraw.
                AnsiConsole.WriteLine();
                continue;
            }

            if (text.Length == 0)
            {
                continue;
            }

            if (text.StartsWith('/'))
            {
                (SlashCommand Command, string Argument)? parsed = SlashCommand.Parse(text);
                if (parsed is null)
                {
                    EventRenderer.Warn($"unknown command: {text.Split(' ')[0]} — try /help");
                    AnsiConsole.WriteLine();
                    continue;
                }

                if (await HandleCommandAsync(parsed.Value.Command, parsed.Value.Argument, input.CancellationToken, ct))
                {
                    continue;
                }

                return;
            }

            await PlayTurnAsync(new PlayerAction(_settings.DefaultActionKind, text), input.CancellationToken, ct);
        }
    }

    /// <summary>Returns false when the player chose to quit.</summary>
    private async Task<bool> OpenMenuAsync(CancellationToken ct)
    {
        while (true)
        {
            MenuResult result = await menu.ShowAsync(_snapshot is not null, ct);
            AnsiConsole.WriteLine();

            switch (result)
            {
                case MenuResult.Quit:
                    return false;

                case MenuResult.Resume when _snapshot is not null:
                    return true;

                case MenuResult.Resume:
                    continue;

                case MenuResult.EditSettings:
                    _settings = settingsEditor.Edit(_settings);
                    continue;

                case MenuResult.Play play:
                    _snapshot = await backend.GetSnapshotAsync(play.CampaignId, ct);

                    if (play.OpeningPrompt is { } opening)
                    {
                        await StartCampaignAsync(play.CampaignId, opening, ct);
                    }
                    else
                    {
                        IReadOnlyList<MessageView> history =
                            await backend.GetHistoryAsync(play.CampaignId, _settings.HistoryReplayCount, ct);
                        EventRenderer.Transcript(history, WrapWidth());
                        ShowParty(force: false);
                    }

                    return true;
            }
        }
    }

    private async Task<bool> HandleCommandAsync(
        SlashCommand command, string argument, CancellationToken turnToken, CancellationToken ct)
    {
        if (command == SlashCommand.Quit)
        {
            return false;
        }

        if (command == SlashCommand.Help)
        {
            WriteHelp();
        }
        else if (command == SlashCommand.Party)
        {
            ShowParty(force: true);
        }
        else if (command == SlashCommand.Look)
        {
            EventRenderer.Info(_snapshot?.PlayerLocation is { } where
                ? $"you are at {where}"
                : "nowhere in particular");
            AnsiConsole.WriteLine();
        }
        else if (command == SlashCommand.History)
        {
            int count = int.TryParse(argument, out int parsed) ? parsed : _settings.HistoryReplayCount;
            if (_snapshot is not null)
            {
                EventRenderer.Transcript(await backend.GetHistoryAsync(_snapshot.CampaignId, count, ct), WrapWidth());
            }
        }
        else if (command == SlashCommand.Clear)
        {
            AnsiConsole.Clear();
        }
        else if (command == SlashCommand.Config)
        {
            _settings = settingsEditor.Edit(_settings);
        }
        else if (command == SlashCommand.Menu)
        {
            return await OpenMenuAsync(ct);
        }
        else if (command == SlashCommand.Say || command == SlashCommand.Do)
        {
            if (argument.Length == 0)
            {
                EventRenderer.Warn($"usage: {command.Usage}");
                AnsiConsole.WriteLine();
            }
            else
            {
                PlayerActionKind kind = command == SlashCommand.Say ? PlayerActionKind.Say : PlayerActionKind.Do;
                await PlayTurnAsync(new PlayerAction(kind, argument), turnToken, ct);
            }
        }

        return true;
    }

    private async Task PlayTurnAsync(PlayerAction action, CancellationToken turnToken, CancellationToken ct)
    {
        if (_snapshot is null)
        {
            return;
        }

        if (_snapshot.GameOver)
        {
            EventRenderer.Warn("this adventure has ended — open the menu to start another");
            AnsiConsole.WriteLine();
            return;
        }

        EventRenderer.PlayerAction(action);

        GameSnapshot before = _snapshot;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(turnToken, ct);

        TurnOutcome outcome = await new TurnRenderer(_settings)
            .RenderAsync(backend.PlayTurnAsync(before.CampaignId, action, linked.Token), linked.Token);

        await RefreshAsync(before, outcome, ct);
    }

    private async Task StartCampaignAsync(int campaignId, string openingPrompt, CancellationToken ct)
    {
        GameSnapshot before = _snapshot ?? await backend.GetSnapshotAsync(campaignId, ct);

        TurnOutcome outcome = await new TurnRenderer(_settings)
            .RenderAsync(backend.StartCampaignAsync(campaignId, openingPrompt, ct), ct);

        await RefreshAsync(before, outcome, ct);
    }

    private async Task RefreshAsync(GameSnapshot before, TurnOutcome outcome, CancellationToken ct)
    {
        if (outcome.Cancelled)
        {
            // The turn was abandoned mid-stream, so nothing was persisted and the snapshot still holds.
            ShowParty(force: false);
            return;
        }

        _snapshot = await backend.GetSnapshotAsync(before.CampaignId, ct);
        EventRenderer.HealthChanges(before, _snapshot);
        EventRenderer.TurnSeparator();
        ShowParty(force: false);
    }

    /// <summary>
    /// The party is redrawn after a turn rather than before every prompt, so commands that produce
    /// their own output are not followed by a redundant panel.
    /// </summary>
    private void ShowParty(bool force)
    {
        if (_snapshot is null || (!force && !_settings.ShowPartyPanel))
        {
            return;
        }

        AnsiConsole.Write(PartyPanel.Build(_snapshot));
        AnsiConsole.WriteLine();
    }

    private int WrapWidth() =>
        _settings.NarrationWidth > 0 ? _settings.NarrationWidth : Math.Max(40, AnsiConsole.Profile.Width - 2);

    private static void WriteBanner()
    {
        AnsiConsole.Write(new FigletText("ChatRPG").LeftJustified().Color(Theme.Accent.Foreground));
        EventRenderer.Info("/help for commands · F2 menu · F3 settings · Ctrl-C cancels a turn");
        AnsiConsole.WriteLine();
    }

    private static void WriteHelp()
    {
        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn(new TableColumn("usage").NoWrap())
            .AddColumn("what");

        foreach (SlashCommand command in SlashCommand.All)
        {
            table.AddRow(new Text(command.Usage, Theme.Accent), new Text(command.Summary, Theme.Muted));
        }

        table.AddRow(new Text("<text>", Theme.Accent), new Text("act — sent as the default action kind", Theme.Muted));
        table.AddRow(new Text("shift+enter", Theme.Accent), new Text("new line without submitting", Theme.Muted));

        AnsiConsole.Write(new Padder(table, new Padding(2, 0, 0, 0)));
        AnsiConsole.WriteLine();
    }
}
