using ChatRPG.Domain.Entities;

namespace ChatRPG.Domain.Exceptions;

public sealed class MalformedGraphException : DomainException
{
    public NarrativeGraph Graph { get; }
    public NarrativeNode? Node { get; }
    public NarrativeEdge? Edge { get; }

    private MalformedGraphException(NarrativeGraph graph, string? message = null) : base(message)
    {
        Graph = graph;
    }

    public MalformedGraphException(NarrativeGraph graph, NarrativeNode node, string? message = null) : this(graph, message)
    {
        Node = node;
    }

    public MalformedGraphException(NarrativeGraph graph, NarrativeEdge edge, string? message = null) : this(graph, message)
    {
        Edge = edge;
    }
}