using ChatRPG.Application.Gameplay;
using ChatRPG.ConsoleUI.Rendering;
using Spectre.Console;

namespace ChatRPG.ConsoleUI.Configuration;

/// <summary>
/// Edits <see cref="PlayerSettings"/> through Spectre prompts. Only ever run between input reads,
/// never while the PrettyPrompt input bar is open.
/// </summary>
public sealed class SettingsEditor(PlayerSettingsStore store)
{
    private const string Done = "← back";

    public PlayerSettings Edit(PlayerSettings settings)
    {
        while (true)
        {
            AnsiConsole.Write(Summary(settings));
            AnsiConsole.WriteLine();

            string choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[grey54]edit which setting?[/]")
                .HighlightStyle(Theme.Accent)
                .AddChoices(
                    "player name",
                    "accent colour",
                    "narration width",
                    "party panel",
                    "autocomplete",
                    "persist history",
                    "default action",
                    "replayed messages",
                    Done));

            settings = choice switch
            {
                "player name" => settings with
                {
                    PlayerName = AnsiConsole.Ask("name", settings.PlayerName)
                },
                "accent colour" => settings with
                {
                    Accent = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title("accent colour")
                        .HighlightStyle(Theme.Accent)
                        .AddChoices(Theme.AccentChoices))
                },
                "narration width" => settings with
                {
                    NarrationWidth = AnsiConsole.Prompt(new TextPrompt<int>("wrap width (0 = terminal width)")
                        .DefaultValue(settings.NarrationWidth)
                        .Validate(w => w is 0 or >= 40
                            ? ValidationResult.Success()
                            : ValidationResult.Error("use 0, or at least 40")))
                },
                "party panel" => settings with
                {
                    ShowPartyPanel = AnsiConsole.Confirm("show the party panel above the prompt?", settings.ShowPartyPanel)
                },
                "autocomplete" => settings with
                {
                    EnableCompletion = AnsiConsole.Confirm("enable autocomplete?", settings.EnableCompletion)
                },
                "persist history" => settings with
                {
                    PersistHistory = AnsiConsole.Confirm("remember input history between runs?", settings.PersistHistory)
                },
                "default action" => settings with
                {
                    DefaultActionKind = AnsiConsole.Prompt(new SelectionPrompt<PlayerActionKind>()
                        .Title("bare input is sent as")
                        .HighlightStyle(Theme.Accent)
                        .AddChoices(PlayerActionKind.Do, PlayerActionKind.Say))
                },
                "replayed messages" => settings with
                {
                    HistoryReplayCount = AnsiConsole.Prompt(new TextPrompt<int>("messages to replay when resuming")
                        .DefaultValue(settings.HistoryReplayCount)
                        .Validate(c => c is >= 0 and <= 50
                            ? ValidationResult.Success()
                            : ValidationResult.Error("use 0–50")))
                },
                _ => settings
            };

            store.Save(settings);
            Theme.Apply(settings);

            if (choice == Done)
            {
                AnsiConsole.WriteLine();
                return settings;
            }
        }
    }

    private static Panel Summary(PlayerSettings settings)
    {
        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn(new TableColumn("k").NoWrap())
            .AddColumn("v");

        table.AddRow(new Text("player name", Theme.Muted), new Text(settings.PlayerName));
        table.AddRow(new Text("accent colour", Theme.Muted), new Text(settings.Accent));
        table.AddRow(new Text("narration width", Theme.Muted),
            new Text(settings.NarrationWidth == 0 ? "terminal width" : settings.NarrationWidth.ToString()));
        table.AddRow(new Text("party panel", Theme.Muted), new Text(settings.ShowPartyPanel ? "on" : "off"));
        table.AddRow(new Text("autocomplete", Theme.Muted), new Text(settings.EnableCompletion ? "on" : "off"));
        table.AddRow(new Text("persist history", Theme.Muted), new Text(settings.PersistHistory ? "on" : "off"));
        table.AddRow(new Text("default action", Theme.Muted), new Text(settings.DefaultActionKind.ToString().ToLowerInvariant()));
        table.AddRow(new Text("replayed messages", Theme.Muted), new Text(settings.HistoryReplayCount.ToString()));

        return new Panel(table)
            .Header($" settings — {Markup.Escape(settings.PlayerName)} ", Justify.Left)
            .Border(BoxBorder.Rounded)
            .BorderStyle(Theme.Accent)
            .Padding(1, 0);
    }
}
