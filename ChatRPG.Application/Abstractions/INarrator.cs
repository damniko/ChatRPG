using ChatRPG.Application.Gameplay;

namespace ChatRPG.Application.Abstractions;

public interface INarrator
{
    IAsyncEnumerable<string> NarrateStreamingAsync(NarrationRequest request, CancellationToken ct = default);

    Task<string> NarrateAsync(NarrationRequest request, CancellationToken ct = default);
}
