using ChatRPG.Domain.Enums;

namespace ChatRPG.Application.Abstractions;

public interface IArchivist
{
    Task<ArchiveResult> ApplyNarrativeChangesAsync(ArchiveRequest request, CancellationToken ct = default);
}

public sealed record ArchiveRequest(
    int CampaignId,
    string GameSummary,
    IReadOnlyList<CharacterView> Characters,
    IReadOnlyList<string> Locations,
    string PlayerInput,
    string Narration);

public sealed record CharacterView(int Id, string Name, string Description, int Health, bool IsPlayer, CharacterType Type);

public sealed record ArchiveResult(
    IReadOnlyList<CharacterChange> CharacterChanges,
    IReadOnlyList<NewCharacter> NewCharacters,
    IReadOnlyList<LocationChange> LocationChanges);

public sealed record CharacterChange(int CharacterId, int HealthDelta, string? NewDescription);
public sealed record NewCharacter(string Name, string Description, CharacterType Type);
public sealed record LocationChange(string Name, string Description, bool IsPlayerHere);
