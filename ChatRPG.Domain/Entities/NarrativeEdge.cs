using System.Text;
using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class NarrativeEdge : IEntity
{
    private NarrativeEdge() { }

    public NarrativeEdge(List<string> conditions, NarrativeNode sourceNode, NarrativeNode targetNode)
    {
        Conditions = conditions;
        SourceNode = sourceNode;
        TargetNode = targetNode;
    }

    public int Id { get; init; }

    public ICollection<string> Conditions { get; private set; } = [];

    public int SourceNodeId { get; private set; }

    public NarrativeNode SourceNode { get; private set; } = null!;

    public int TargetNodeId { get; private set; }

    public NarrativeNode TargetNode { get; private set; } = null!;

    public Status EdgeStatus { get; set; } = Status.Unvisited;

    public enum Status
    {
        Unvisited,
        Visited
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{EdgeStatus} Edge from {SourceNode.Name} to {TargetNode.Name} with conditions: ");
        sb.Append(string.Join(", ", Conditions));

        return sb.ToString();
    }
}
