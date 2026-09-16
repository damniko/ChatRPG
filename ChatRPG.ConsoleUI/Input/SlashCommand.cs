namespace ChatRPG.ConsoleUI.Input;

public sealed record SlashCommand(string Name, string Summary, string Usage)
{
    public static readonly SlashCommand Help = new("/help", "list the commands", "/help");
    public static readonly SlashCommand Say = new("/say", "speak in character", "/say <words>");
    public static readonly SlashCommand Do = new("/do", "take an action", "/do <action>");
    public static readonly SlashCommand Party = new("/party", "show the party", "/party");
    public static readonly SlashCommand Look = new("/look", "show where you are", "/look");
    public static readonly SlashCommand History = new("/history", "replay recent messages", "/history [count]");
    public static readonly SlashCommand Menu = new("/menu", "open the main menu", "/menu");
    public static readonly SlashCommand Config = new("/config", "edit your settings", "/config");
    public static readonly SlashCommand Clear = new("/clear", "clear the screen", "/clear");
    public static readonly SlashCommand Quit = new("/quit", "leave the game", "/quit");

    public static readonly IReadOnlyList<SlashCommand> All =
        [Help, Say, Do, Party, Look, History, Menu, Config, Clear, Quit];

    /// <summary>Splits input into a command and its argument, or returns null when it is not a command.</summary>
    public static (SlashCommand Command, string Argument)? Parse(string input)
    {
        string trimmed = input.TrimStart();
        if (!trimmed.StartsWith('/'))
        {
            return null;
        }

        int space = trimmed.IndexOf(' ');
        string name = space < 0 ? trimmed : trimmed[..space];
        string argument = space < 0 ? string.Empty : trimmed[(space + 1)..].Trim();

        SlashCommand? command = All.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return command is null ? null : (command, argument);
    }
}
