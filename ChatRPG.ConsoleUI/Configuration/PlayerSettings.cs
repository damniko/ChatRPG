using ChatRPG.Application.Gameplay;

namespace ChatRPG.ConsoleUI.Configuration;

/// <summary>
/// Player-facing preferences, stored next to the user profile and editable from <c>/config</c>.
/// API keys and connection strings stay in appsettings.json and user-secrets.
/// </summary>
public sealed record PlayerSettings
{
    /// <summary>Identifies the player across runs. Generated once on first launch.</summary>
    public Guid IdentityId { get; init; } = Guid.Empty;

    public string PlayerName { get; init; } = Environment.UserName;

    /// <summary>Spectre colour name used for panel borders and headings.</summary>
    public string Accent { get; init; } = "mediumpurple2";

    /// <summary>Hard wrap for narration. 0 follows the terminal width.</summary>
    public int NarrationWidth { get; init; }

    public bool ShowPartyPanel { get; init; } = true;

    public bool EnableCompletion { get; init; } = true;

    public bool PersistHistory { get; init; } = true;

    /// <summary>Which action kind bare input (no slash command) is sent as.</summary>
    public PlayerActionKind DefaultActionKind { get; init; } = PlayerActionKind.Do;

    public int HistoryReplayCount { get; init; } = 6;
}
