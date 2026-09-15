using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Application.Abstractions;

public interface IArchivist
{
    Task<ArchiveResult> ApplyNarrativeChangesAsync(ArchiveRequest request, CancellationToken ct = default);
    Task AppendMessagesAsync(Campaign campaign, string playerInput, string narration, ActionRuling? ruling, string? epilogue, CancellationToken ct = default);
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
    string UpdatedSummary,
    IReadOnlyList<CharacterChange> CharacterChanges,
    IReadOnlyList<NewCharacter> NewCharacters,
    IReadOnlyList<LocationChange> LocationChanges);

public sealed record CharacterChange(int CharacterId, int HealthDelta, string? NewDescription);
public sealed record NewCharacter(string Name, string Description, CharacterType Type);

/// <summary>A location the archivist created or redescribed. Named, as it may not exist yet.</summary>
public sealed record LocationChange(string Name, string Description, bool IsPlayerHere);
