using ChatRPG.Domain.Enums;

namespace ChatRPG.Application.Abstractions;

public interface IArchivist
{
    Task<ArchiveResult> ApplyNarrativeChangesAsync(ArchiveRequest request, CancellationToken ct = default);
}

public abstract record ArchiveRequest(
    int CampaignId,
    string GameSummary,
    IReadOnlyList<CharacterView> Characters,
    IReadOnlyList<string> Locations)
{
    /// <summary>The player acted and the GM narrated the result.</summary>
    public sealed record Turn(
        int CampaignId,
        string GameSummary,
        IReadOnlyList<CharacterView> Characters,
        IReadOnlyList<string> Locations,
        string PlayerInput,
        string Narration) : ArchiveRequest(CampaignId, GameSummary, Characters, Locations);

    /// <summary>The first narration of a campaign; there is no player input yet.</summary>
    public sealed record Opening(
        int CampaignId,
        string GameSummary,
        IReadOnlyList<CharacterView> Characters,
        IReadOnlyList<string> Locations,
        string Narration) : ArchiveRequest(CampaignId, GameSummary, Characters, Locations);
}

public sealed record CharacterView(int Id, string Name, string Description, int Health, bool IsPlayer, CharacterType Type);

public sealed record ArchiveResult(
    IReadOnlyList<CharacterChange> CharacterChanges,
    IReadOnlyList<NewCharacter> NewCharacters,
    IReadOnlyList<LocationChange> LocationChanges);

public sealed record CharacterChange(int CharacterId, int HealthDelta, string? NewDescription);
public sealed record NewCharacter(string Name, string Description, CharacterType Type);
public sealed record LocationChange(string Name, string Description, bool IsPlayerHere);
