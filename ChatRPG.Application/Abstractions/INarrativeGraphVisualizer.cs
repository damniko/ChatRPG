using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface INarrativeGraphVisualizer
{
    Task<GraphVisualization> GenerateAndSaveAsync(NarrativeGraph graph, CancellationToken ct = default);
}