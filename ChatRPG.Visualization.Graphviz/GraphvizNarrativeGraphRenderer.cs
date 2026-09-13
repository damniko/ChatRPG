using System.Text;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;
using ChatRPG.Domain.Exceptions;
using Rubjerg.Graphviz;

namespace ChatRPG.Visualization.Graphviz;

internal class GraphvizNarrativeGraphRenderer : INarrativeGraphRenderer
{
     public Task<byte[]> RenderAsync(NarrativeGraph graph, VisualizationFormat format, CancellationToken ct = default)
    {
        var root = BuildGraph(graph);
        byte[] bytes = format switch
        {
            VisualizationFormat.Svg => Encoding.UTF8.GetBytes(root.ToSvgString()),
            VisualizationFormat.Png => root.ToPngBytes(),
            VisualizationFormat.Pdf => root.ToPdfBytes(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
        return Task.FromResult(bytes);
    }

    private static RootGraph BuildGraph(NarrativeGraph graph)
    {
        var root = RootGraph.CreateNew(GraphType.Directed, $"Graph ID {graph.Id}");
        Node.IntroduceAttribute(root, "shape", "circle");
        Edge.IntroduceAttribute(root, "label", "");

        foreach (var node in graph.Nodes)
        {
            var newNode = root.GetOrAddNode(node.Id.ToString());

            // If the node is the start node, make it a point,
            // otherwise set the label to the story content
            if (node.Id == graph.GetStartNode()?.Id)
            {
                newNode.SetAttribute("shape", "point");
            }
            else
            {
                newNode.SafeSetAttribute("label", node.Name, "");

                switch (node.NodeStatus)
                {
                    case NarrativeNode.Status.Undiscovered:
                        newNode.SetAttribute("color", "red");
                        break;
                    case NarrativeNode.Status.Ongoing:
                        newNode.SetAttribute("color", "yellow");
                        break;
                    case NarrativeNode.Status.Completed:
                        newNode.SetAttribute("color", "green");
                        break;
                    default:
                        throw new MalformedGraphException(graph, node, $"Unexpected status '{node.NodeStatus}'");
                }
            }

            if (node.Name == "End")
            {
                newNode.SetAttribute("shape", "doublecircle");
            }
        }

        foreach (var node in graph.Nodes)
        {
            foreach (var edge in node.Edges)
            {
                var source = root.GetNode(edge.SourceNodeId.ToString());
                var target = root.GetNode(edge.TargetNodeId.ToString());

                var newEdge = root.GetOrAddEdge(source, target, edge.Id.ToString());
                newEdge.SafeSetAttribute("label", string.Join(", ", edge.Conditions), "");

                switch (edge.EdgeStatus)
                {
                    case NarrativeEdge.Status.Unvisited:
                        newEdge.SetAttribute("color", "red");
                        break;
                    case NarrativeEdge.Status.Visited:
                        newEdge.SetAttribute("color", "green");
                        break;
                    default:
                        throw new MalformedGraphException(graph, edge, $"Unexpected status '{edge.EdgeStatus}'");
                }
            }
        }

        return root;
    }
}