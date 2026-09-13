using ChatRPG.Domain.Entities.Abstractions;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Domain.Entities;

public class GraphVisualization : IEntity
{
    private GraphVisualization() {}

    public GraphVisualization(NarrativeGraph graph, string location, VisualizationFormat format)
    {
        NarrativeGraph = graph;
        Location = location;
        Format = format;
    }

    public int Id { get; init; }
    public NarrativeGraph NarrativeGraph { get; private set; }
    public string Location { get; private set; }
    public VisualizationFormat Format { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public string ContentType => Format.ContentType();
}