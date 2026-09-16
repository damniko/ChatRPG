namespace ChatRPG.ConsoleUI.Session;

/// <summary>
/// A read-only projection of a campaign, taken inside the DI scope that loaded it so the UI never
/// touches EF entities after their <c>DbContext</c> is gone.
/// </summary>
public sealed record GameSnapshot(
    int CampaignId,
    string Title,
    bool IsOpenWorld,
    bool GameOver,
    string PlayerName,
    string? PlayerLocation,
    IReadOnlyList<PartyMember> Party,
    IReadOnlyList<string> CharacterNames,
    IReadOnlyList<string> LocationNames);

public sealed record PartyMember(string Name, int CurrentHealth, int MaxHealth, string Location, bool IsPlayer);

public sealed record CampaignSummary(int Id, string Title, string PlayerName, DateTime StartedOn, bool GameOver);

public sealed record NewCampaignRequest(
    string Title,
    string StartScenario,
    bool IsOpenWorld,
    string CharacterName,
    string CharacterDescription,
    byte[]? ScenarioDocument);
