using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class AddNodeTool(
    IToolDataTextParser parser,
    NarrativeGraph graph,
    IToolDescriptionCatalog descriptions,
    IToolDataValidator<ToolData.AddNode> nodeValidator,
    EdgeConsistencyValidator edgeConsistencyValidator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.AddNode))
{
    private const string ToolName = "addnodetool";

    public override Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.AddNode>(input, out var toolData, out string? error))
        {
            return Task.FromResult($"Invalid input syntax: {error}");
        }
        var addNode = toolData!;

        if (!nodeValidator.IsValid(addNode, out var errors))
        {
            return Task.FromResult($"Invalid input: {string.Join(", ", errors)}");
        }
        
        if (graph.Nodes.Any(n => n.Name == addNode.Name))
        {
            return Task.FromResult($"Node with name '{addNode.Name}' already exists. Please provide a unique name.");
        }

        var node = new NarrativeNode(addNode.Name, addNode.StoryContent, graph);
        var edgesToAdd = new List<NarrativeEdge>();
        var existingNodesDict = graph.Nodes.ToDictionary(n => n.Name);
        var edgeErrorList = new List<string>();

        foreach (var edgeData in addNode.Edges)
        {
            existingNodesDict.TryGetValue(edgeData.SourceNodeName, out var sourceNode);
            existingNodesDict.TryGetValue(edgeData.TargetNodeName, out var targetNode);
            sourceNode ??= edgeData.SourceNodeName == node.Name ? node : null;
            targetNode ??= edgeData.TargetNodeName == node.Name ? node : null;

            if (!EdgeConsistencyValidator.IsValid(sourceNode, targetNode, edgeData, out var edgeErrors))
            {
                edgeErrorList.AddRange(edgeErrors);
                continue;
            }

            var edge = new NarrativeEdge(edgeData.Conditions, sourceNode!, targetNode!);
            edgesToAdd.Add(edge);
        }

        if (edgeErrorList.Count > 0)
        {
            return Task.FromResult($"Invalid input provided for the node. Please correct the following errors: {string.Join(", ", edgeErrorList)}");
        }
        
        graph.AddNode(node);
        edgesToAdd.ForEach(edge => edge.SourceNode.Edges.Add(edge));

        return Task.FromResult(
            $"The graph has been updated. Examine the graph to determine if additional edges should be added based on the newly added node. From now on, use the updated graph:\n{NarrativeGraphFormatter.Format(graph)}");
    }
}
