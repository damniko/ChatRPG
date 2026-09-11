using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IGraphNavigator
{
    Task<string> ReviewGraphAsync(
        Campaign campaign,
        string playerInput,
        AdherenceVerdict verdict,
        CancellationToken ct = default);
}
