using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Gameplay;
using ChatRPG.ConsoleUI.Session;
using ChatRPG.Domain.Entities;
using Spectre.Console;

namespace ChatRPG.ConsoleUI.Rendering;

internal static class EventRenderer
{
    public static void PlayerAction(PlayerAction action)
    {
        string verb = action.Kind == PlayerActionKind.Say ? "say" : "do";
        AnsiConsole.Write(new Markup(
            $"[{Theme.PlayerAction.Foreground.ToMarkup()}]  you {verb}:[/] {Markup.Escape(action.Text)}"));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
    }

    public static void Ruling(ActionRuling ruling)
    {
        if (ruling.Permission == ActionPermission.Disallowed)
        {
            // InputRejected follows with the same reasoning in its own panel.
            return;
        }

        (string label, Style style) = ruling.Permission switch
        {
            ActionPermission.Allowed => ("allowed", Theme.Allowed),
            _ => ("complicated", Theme.Conditional)
        };

        AnsiConsole.Write(new Markup(
            $"  [{style.Foreground.ToMarkup()}]▸ {label}[/] [grey54]{Markup.Escape(Shorten(ruling.Reasoning, 160))}[/]"));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
    }

    public static void InputRejected(string reasoning)
    {
        AnsiConsole.Write(new Panel(new Markup($"[grey85]{Markup.Escape(reasoning)}[/]"))
            .Header(" the world resists ", Justify.Left)
            .Border(BoxBorder.Rounded)
            .BorderStyle(Theme.Disallowed)
            .Padding(1, 0));
        AnsiConsole.WriteLine();
    }

    public static void Epilogue(string epilogue)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(new Text(epilogue, Theme.Epilogue))
            .Header(" the adventure ends ", Justify.Left)
            .Border(BoxBorder.Double)
            .BorderStyle(Theme.Epilogue)
            .Padding(1, 0));
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Health changes are not reported as turn events — the combat tools resolve them inside the
    /// narrator's ReAct loop — so they are recovered by diffing the snapshots around the turn.
    /// </summary>
    public static void HealthChanges(GameSnapshot before, GameSnapshot after)
    {
        Dictionary<string, PartyMember> previous = before.Party.ToDictionary(m => m.Name);
        var lines = new List<string>();

        foreach (PartyMember member in after.Party)
        {
            if (!previous.TryGetValue(member.Name, out PartyMember? was) || was.CurrentHealth == member.CurrentHealth)
            {
                continue;
            }

            int delta = member.CurrentHealth - was.CurrentHealth;
            Style style = delta < 0 ? Theme.Damage : Theme.Healing;
            string sign = delta < 0 ? "−" : "+";
            lines.Add($"  [{style.Foreground.ToMarkup()}]{sign}{Math.Abs(delta)} HP[/] " +
                      $"[grey54]{Markup.Escape(member.Name)} → {member.CurrentHealth}/{member.MaxHealth}[/]");
        }

        foreach (string name in previous.Keys.Except(after.Party.Select(m => m.Name)))
        {
            lines.Add($"  [{Theme.Damage.Foreground.ToMarkup()}]✝ {Markup.Escape(name)} is gone[/]");
        }

        if (lines.Count == 0)
        {
            return;
        }

        foreach (string line in lines)
        {
            AnsiConsole.Write(new Markup(line));
            AnsiConsole.WriteLine();
        }
        AnsiConsole.WriteLine();
    }

    public static void Transcript(IReadOnlyList<MessageView> messages, int width)
    {
        if (messages.Count == 0)
        {
            return;
        }

        AnsiConsole.Write(new Rule("[grey54]previously[/]") { Justification = Justify.Left, Style = Theme.Muted });
        AnsiConsole.WriteLine();

        foreach (MessageView message in messages)
        {
            switch (message)
            {
                case PlayerMessageView player:
                    AnsiConsole.Write(new Markup(
                        $"[{Theme.PlayerAction.Foreground.ToMarkup()}]  you:[/] [grey70]{Markup.Escape(player.Content)}[/]"));
                    AnsiConsole.WriteLine();
                    AnsiConsole.WriteLine();
                    break;

                case NarrationMessageView narration:
                    var writer = new NarrationWriter(width);
                    writer.Write(narration.Content);
                    writer.Flush();
                    AnsiConsole.WriteLine();
                    break;
            }
        }
    }

    public static void TurnSeparator()
    {
        AnsiConsole.Write(new Rule { Style = new Style(Color.Grey23) });
        AnsiConsole.WriteLine();
    }

    public static void Info(string message) =>
        AnsiConsole.MarkupLine($"[grey54]  {Markup.Escape(message)}[/]");

    public static void Warn(string message) =>
        AnsiConsole.MarkupLine($"[{Theme.Disallowed.Foreground.ToMarkup()}]  {Markup.Escape(message)}[/]");

    private static string Shorten(string text, int max) =>
        text.Length <= max ? text : string.Concat(text.AsSpan(0, max - 1).TrimEnd(), "…");
}
