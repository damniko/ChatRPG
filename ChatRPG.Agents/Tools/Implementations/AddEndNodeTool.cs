using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class AddEndNodeTool(
    NarrativeGraph graph,
    IToolDescriptionCatalog descriptions,
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.AddEndNode> validator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.AddEndNode))
{
    private const string ToolName = "addendnode";

    public override Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.AddEndNode>(input, out var toolData, out string? error))
        {
            return Task.FromResult($"Invalid input syntax: {error}");
        }
        var addEndNode = toolData!;

        if (!validator.IsValid(addEndNode, out var errors))
        {
            return Task.FromResult($"Invalid input: {string.Join(", ", errors)}");
        }

        var sourceNode = graph.Nodes.FirstOrDefault(n => n.Name == addEndNode.SourceNodeName);
        if (sourceNode == null)
        {
            return Task.FromResult($"Source node with name '{addEndNode.SourceNodeName}' was not found.");
        }
        
        var targetNode = graph.Nodes.FirstOrDefault(n => n.Name == "End");
        if (targetNode != null && sourceNode!.Edges.Any(e => e.TargetNode.Name == targetNode.Name))
        {
            return Task.FromResult($"Failed to add edge to end node: An edge already exists between {sourceNode.Name} and {targetNode.Name}.");
        }

        if (sourceNode == targetNode)
        {
            return Task.FromResult($"Node {sourceNode.Name} cannot have an edge to itself.");
        }

        if (targetNode == null)
        {
            targetNode = new NarrativeNode("End", "", graph);
            graph.AddNode(targetNode);
        }
        
        sourceNode.Edges.Add(new NarrativeEdge(addEndNode.Conditions, sourceNode, targetNode));
        return Task.FromResult($"The graph has been updated. From now on, use the updated graph:\n{NarrativeGraphFormatter.Format(graph)}");
    }
}
