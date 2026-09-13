using System.Text.Json;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Llm;

internal static class NarrativeGraphFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string Format(NarrativeGraph graph)
    {
        return JsonSerializer.Serialize(ToPromptModel(graph), JsonOptions);
    }

    public static string Format(NarrativeEdge edge)
    {
        return JsonSerializer.Serialize(ToPromptModel(edge), JsonOptions);
    }

    private static GraphModel ToPromptModel(NarrativeGraph graph)
    {
        return new GraphModel([.. graph.Nodes.Select(ToPromptModel)]);
    }

    private static NodeModel ToPromptModel(NarrativeNode node)
    {
        return new NodeModel(
            node.Name,
            node.StoryContent,
            [.. node.Edges.Select(ToPromptModel)],
            node.NodeStatus.ToString());
    }

    private static EdgeModel ToPromptModel(NarrativeEdge edge)
    {
        return new EdgeModel(
            [.. edge.Conditions],
            edge.SourceNode.Name,
            edge.TargetNode.Name,
            edge.EdgeStatus.ToString());
    }

    private sealed record GraphModel(IReadOnlyCollection<NodeModel> Nodes);

    private sealed record NodeModel(
        string Name,
        string StoryContent,
        IReadOnlyCollection<EdgeModel> Edges,
        string NodeStatusCategory);

    private sealed record EdgeModel(
        IReadOnlyCollection<string> Conditions,
        string SourceNodeName,
        string TargetNodeName,
        string EdgeStatusCategory);
}
