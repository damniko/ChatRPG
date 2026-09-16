using ChatRPG.ConsoleUI.Session;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ChatRPG.ConsoleUI.Rendering;

internal static class PartyPanel
{
    private const int BarWidth = 12;

    public static IRenderable Build(GameSnapshot snapshot)
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("name").NoWrap())
            .AddColumn(new TableColumn("hp").NoWrap())
            .AddColumn(new TableColumn("where"));

        foreach (PartyMember member in snapshot.Party)
        {
            Style nameStyle = member.IsPlayer ? Theme.Accent : Theme.Muted;
            table.AddRow(
                new Text(member.Name, nameStyle),
                HealthBar(member),
                new Text(member.Location, Theme.Muted));
        }

        if (snapshot.Party.Count == 0)
        {
            table.AddRow(new Text("(nobody here)", Theme.Muted), new Text(""), new Text(""));
        }

        return new Panel(table)
            .Header($" Party — {Markup.Escape(snapshot.Title)} ", Justify.Left)
            .Border(BoxBorder.Rounded)
            .BorderStyle(Theme.Accent)
            .Padding(1, 0);
    }

    private static IRenderable HealthBar(PartyMember member)
    {
        int max = Math.Max(1, member.MaxHealth);
        int current = Math.Clamp(member.CurrentHealth, 0, max);
        int filled = (int)Math.Round(current / (double)max * BarWidth);

        Style style = current == 0 ? Theme.Damage
            : current <= max / 4 ? Theme.Damage
            : current <= max / 2 ? Theme.Conditional
            : Theme.Healing;

        var bar = new Markup(
            $"[{style.Foreground.ToMarkup()}]{new string('█', filled)}[/]" +
            $"[grey30]{new string('░', BarWidth - filled)}[/] " +
            $"{current}/{max}");

        return bar;
    }
}
