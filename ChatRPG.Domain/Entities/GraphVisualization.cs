using ChatRPG.Domain.Entities.Abstractions;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Domain.Entities;

public class GraphVisualization : IEntity
{
    private GraphVisualization() {}

    public GraphVisualization(NarrativeGraph graph, string location, VisualizationFormat format, int revision)
    {
        NarrativeGraph = graph;
        Location = location;
        Format = format;
        Revision = revision;
    }

    public int Id { get; init; }
    public NarrativeGraph NarrativeGraph { get; private set; }
    public string Location { get; private set; }
    public VisualizationFormat Format { get; private set; }
    // TODO: Set CreatedAt as shadow property
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public int Revision { get; private set; }

    public string ContentType => Format.ContentType();
}