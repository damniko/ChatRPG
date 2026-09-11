using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tools.Validators;

// TODO: Abstract this
internal sealed class EdgeConsistencyValidator
{
    public static bool IsValid(NarrativeNode? sourceNode, NarrativeNode? targetNode, ToolData.AddEdge edge, out List<string> errors)
    {
        errors = [];
        if (targetNode is null)
        {
            errors.Add($"Target node with name {edge.TargetNodeName} not found.");
        }

        if (sourceNode is null)
        {
            errors.Add($"Source node with name {edge.SourceNodeName} not found.");
        }
        else if (sourceNode.Name == "End")
        {
            errors.Add($"Node {sourceNode.Name} cannot have an edge to another node.");
        }
        else if (sourceNode == targetNode)
        {
            errors.Add($"Node {sourceNode.Name} cannot have an edge to itself.");
        }
        else if (targetNode is not null && sourceNode.Edges.Any(e => e.TargetNode == targetNode))
        {
            errors.Add($"An edge already exists between {edge.SourceNodeName} and {edge.TargetNodeName}.");
        }

        return errors.Count == 0;
    }

}
