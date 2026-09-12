using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

// TODO: ChatRPG.GraphVisualization project
public interface INarrativeGraphVisualizer
{
    void Visualize(NarrativeGraph graph);
}
