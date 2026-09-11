using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IStartScenarioReader
{
    Task<IReadOnlyList<StartScenario>> ListAsync(CancellationToken ct = default);
}
