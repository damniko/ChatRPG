using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IStartingScenarioGenerator
{
    Task<string> GenerateAsync(Campaign campaign, CancellationToken ct = default);
}
