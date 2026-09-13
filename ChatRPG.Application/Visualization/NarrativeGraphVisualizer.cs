using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Configuration;
using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;
using Microsoft.Extensions.Options;

namespace ChatRPG.Application.Visualization;

public class NarrativeGraphVisualizer(
    INarrativeGraphRenderer renderer,
    IVisualizationStore store,
    IOptions<ApplicationOptions> options) : INarrativeGraphVisualizer
{
    public async Task<GraphVisualization> GenerateAndSaveAsync(NarrativeGraph graph, CancellationToken ct = default)
    {
        VisualizationFormat format = options.Value.GraphVisualizationFormat;
        // TODO: Thread safe revision counter, db sequence?
        int revision = graph.Visualizations.Count + 1;
        string location = $"graphs/{graph.Id}/{Guid.NewGuid()}.{format.Extension()}";
        
        byte[] content = await renderer.RenderAsync(graph, format, ct);
        await store.SaveAsync(location, content, ct);

        return new GraphVisualization(graph, location, format, revision);
    }
}