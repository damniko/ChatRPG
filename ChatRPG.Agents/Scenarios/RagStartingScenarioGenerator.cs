using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Scenarios;

/// <summary>
/// Starting scenario generator based on RAG (Retrieval-Augmented Generation).
/// </summary>
public class RagStartingScenarioGenerator : IStartingScenarioGenerator
{
    public Task<string> GenerateAsync(Campaign campaign, CancellationToken ct = default) => throw new NotImplementedException();
}
