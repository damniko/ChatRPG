using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface INarrativeGraphScribe
{
    Task<NarrativeGraph> ScribeAsync(byte[] scenarioPdf, IProgress<int>? progress = null, CancellationToken ct = default);
}
