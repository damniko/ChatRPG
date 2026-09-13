using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class NarrativeGraph : IEntity
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    [JsonIgnore]
    public ICollection<GraphVisualization> Visualizations { get; init; } = [];
    
    [JsonIgnore]
    public int Id { get; init; }

    [JsonIgnore] public ICollection<Campaign> Campaigns { get; private set; } = [];

    public HashSet<NarrativeNode> Nodes { get; private set; } = [];

    public void AddNode(NarrativeNode node)
    {
        Nodes.Add(node);
    }

    public List<NarrativeNode> GetOutgoingNodes(NarrativeNode node)
    {
        return [.. node.Edges.Select(edge => edge.TargetNode)];
    }

    public List<NarrativeNode> GetIncomingNodes(NarrativeNode targetNode)
    {
        return [.. Nodes.Where(n => n.Edges.Any(edge => edge.TargetNode == targetNode))];
    }

    public NarrativeNode? GetEndNode()
    {
        return Nodes.FirstOrDefault(node => node.Name == "End");
    }

    public NarrativeNode? GetStartNode()
    {
        return Nodes.FirstOrDefault(node => GetIncomingNodes(node).Count == 0);
    }

    public List<NarrativeNode> GetNodesWithStatus(NarrativeNode.Status status)
    {
        return Nodes.Where(n => n.NodeStatus == status).ToList();
    }

    public NarrativeNode InitializeStartNode()
    {
        var startNode = new NarrativeNode("Start", "", this)
        {
            NodeStatus = NarrativeNode.Status.Ongoing
        };
        Nodes.Add(startNode);
        return startNode;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, _jsonSerializerOptions);
    }

    public void PrintGraph()
    {
        Console.WriteLine(this);
    }

    public override string ToString()
    {
        var startNode = GetStartNode();
        if (startNode == null)
        {
            return "Warning: No start node found. Create a node with no incoming edges.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Start Node: {startNode}\n");
        var visited = new HashSet<NarrativeNode>();

        Dfs(startNode);
        return sb.ToString();

        void Dfs(NarrativeNode node)
        {
            if (!visited.Add(node)) return; // Skip if already visited
            sb.AppendLine(node.ToString());
            foreach (var edge in node.Edges)
            {
                sb.AppendLine($"  {edge}");
                Dfs(edge.TargetNode); // Recursively visit adjacent nodes
            }
        }
    }
}
