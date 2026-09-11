using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IArchivist
{
    Task ApplyNarrativeChangesAsync(Campaign campaign, string playerInput, string narration, CancellationToken ct = default);
    Task AppendMessagesAsync(Campaign campaign, string playerInput, string narration, AdherenceVerdict? verdict, string? epilogue, CancellationToken ct = default);
}
