using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class AddEdgeTool(
    IToolDataTextParser parser,
    NarrativeGraph graph,
    IToolDescriptionCatalog descriptions,
    IToolDataValidator<ToolData.AddEdge> edgeValidator,
    EdgeConsistencyValidator edgeConsistencyValidator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.AddEdge))
{
    private const string ToolName = "addedgetool";

    public override Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.AddEdge>(input, out var toolData, out string? error))
        {
            return Task.FromResult($"Invalid input syntax: {error}");
        }
        var addEdge = toolData!;

        if (!edgeValidator.IsValid(addEdge, out var errors))
        {
            return Task.FromResult($"Invalid input: {string.Join(", ", errors)}");
        }
        
        var existingNodesDict = graph.Nodes.ToDictionary(n => n.Name);

        existingNodesDict.TryGetValue(addEdge.SourceNodeName, out var sourceNode);
        existingNodesDict.TryGetValue(addEdge.TargetNodeName, out var targetNode);

        if (!EdgeConsistencyValidator.IsValid(sourceNode, targetNode, addEdge, out var edgeErrors))
        {
            return Task.FromResult($"Invalid input provided for the edge. Please correct the following errors: {string.Join(", ", edgeErrors)}");
        }

        var edge = new NarrativeEdge(addEdge.Conditions, sourceNode!, targetNode!);
        sourceNode!.Edges.Add(edge);

        return Task.FromResult($"The graph has been updated. From now on, use the updated graph:\n{NarrativeGraphFormatter.Format(graph)}");
    }
}
