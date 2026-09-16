using ChatRPG.ConsoleUI.Configuration;
using Spectre.Console;

namespace ChatRPG.ConsoleUI.Rendering;

internal static class Theme
{
    public static Style Accent { get; private set; } = new(Color.MediumPurple2);

    public static readonly Style Muted = new(Color.Grey54);
    public static readonly Style PlayerAction = new(Color.DeepSkyBlue1);
    public static readonly Style Allowed = new(Color.Green3);
    public static readonly Style Conditional = new(Color.Khaki1);
    public static readonly Style Disallowed = new(Color.Orange3);
    public static readonly Style Damage = new(Color.Red3);
    public static readonly Style Healing = new(Color.Green3);
    public static readonly Style Epilogue = new(Color.Plum2, decoration: Decoration.Italic);

    public static void Apply(PlayerSettings settings)
    {
        Accent = new Style(Colours.TryGetValue(settings.Accent, out Color colour) ? colour : Color.MediumPurple2);
    }

    private static readonly Dictionary<string, Color> Colours = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mediumpurple2"] = Color.MediumPurple2,
        ["deepskyblue1"] = Color.DeepSkyBlue1,
        ["green3"] = Color.Green3,
        ["khaki1"] = Color.Khaki1,
        ["orange3"] = Color.Orange3,
        ["red3"] = Color.Red3,
        ["gold1"] = Color.Gold1,
        ["aquamarine1"] = Color.Aquamarine1,
        ["hotpink"] = Color.HotPink,
        ["steelblue"] = Color.SteelBlue
    };

    public static IReadOnlyCollection<string> AccentChoices => Colours.Keys.ToArray();
}
