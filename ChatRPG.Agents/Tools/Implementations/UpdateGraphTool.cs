using System.Text;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;
using LangChain.Providers;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class UpdateGraphTool(
    Campaign campaign,
    string gameSummary,
    ActionRuling ruling,
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IToolDescriptionCatalog descriptions,
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.UpdateGraph> validator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.UpdateGraph))
{
    private const string ToolName = "updategraphtool";
    private const double EdgeCheckTemperature = 0.1;
    private const int EdgeCheckMaxRetries = 3;
    
    public override async Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.UpdateGraph>(input, out var toolData, out string? error))
        {
            return $"Invalid input syntax: {error}";
        }
        var updateGraph = toolData!;

        if (!validator.IsValid(updateGraph, out var errors))
            return $"Invalid input: {string.Join(", ", errors)}";

        var srcNodeSearch = SearchSourceNode(updateGraph.SourceNodeName!, updateGraph.TargetNodeName!);
        var targetNodeSearch = SearchTargetNode(updateGraph.TargetNodeName!);

        if (srcNodeSearch.IsFailure || targetNodeSearch.IsFailure)
        {
            return $"Invalid input: {string.Join(", ", [.. srcNodeSearch.Errors, .. targetNodeSearch.Errors])}";
        }
        
        var edgeSearch = SearchEdge(srcNodeSearch.Value!, targetNodeSearch.Value!);
        if (edgeSearch.IsFailure)
            return $"Invalid input: {string.Join(", ", edgeSearch.Errors)}";

        var conditionsCheck = await CheckEdgeConditions(edgeSearch.Value!, ct);
        if (conditionsCheck.IsFailure)
        {
            return "Failed to check conditions for updating the graph. Please try again";
        }

        if (conditionsCheck.Value!.Conditions!.Any(condition => !condition.Value))
        {
            return $"Some conditions were not met for updating the graph, and the node {targetNodeSearch.Value!.Name} should not be set to ongoing. Here are the conditions and their evaluations:\n{string.Join("\n", conditionsCheck.Value!.Conditions.Select(ec => $"{ec.Key}: {ec.Value}"))}";
        }
        
        // All conditions were met, so we can update the graph.
        edgeSearch.Value!.EdgeStatus = NarrativeEdge.Status.Visited;
        srcNodeSearch.Value!.NodeStatus = NarrativeNode.Status.Completed;
        targetNodeSearch.Value!.NodeStatus = targetNodeSearch.Value.Edges.Count == 0 
            ? NarrativeNode.Status.Completed
            : NarrativeNode.Status.Ongoing;
        
        return $"Graph updated successfully: Source node '{srcNodeSearch.Value.Name}' is completed, and target node '{targetNodeSearch.Value.Name}' is {targetNodeSearch.Value!.NodeStatus}. Updated graph:\n{NarrativeGraphFormatter.Format(campaign.NarrativeGraph!)}";
    }

    private Result<NarrativeNode> SearchSourceNode(string sourceNodeName, string targetNodeName)
    {
        var srcNode = campaign.NarrativeGraph!.Nodes.FirstOrDefault(n => n.Name == sourceNodeName);
        if (srcNode is null)
            return Result<NarrativeNode>.Failure($"Failed to find source node with name '{sourceNodeName}'.");

        return srcNode.NodeStatus == NarrativeNode.Status.Undiscovered 
            ? Result<NarrativeNode>.Failure($"Cannot update graph: Source node {sourceNodeName} is not discovered, so {targetNodeName} cannot be marked as ongoing.")
            : Result<NarrativeNode>.Success(srcNode);
    }

    private Result<NarrativeNode> SearchTargetNode(string targetNodeName)
    {
        var targetNode = campaign.NarrativeGraph!.Nodes.FirstOrDefault(n => n.Name == targetNodeName);
        if (targetNode is null)
            return Result<NarrativeNode>.Failure($"Failed to find target node with name '{targetNodeName}'.");

        return targetNode.NodeStatus != NarrativeNode.Status.Undiscovered
            ? Result<NarrativeNode>.Failure($"Cannot update graph: Target node '{targetNodeName}' is already {targetNode.NodeStatus}.") 
            : Result<NarrativeNode>.Success(targetNode);
    }

    private static Result<NarrativeEdge> SearchEdge(NarrativeNode srcNode, NarrativeNode targetNode)
    {
        var edge = srcNode.Edges.FirstOrDefault(e => e.TargetNode == targetNode);
        if (edge is null)
            return Result<NarrativeEdge>.Failure($"Failed to find edge between '{srcNode.Name}' and '{targetNode.Name}'.");

        return edge.EdgeStatus == NarrativeEdge.Status.Visited
            ? Result<NarrativeEdge>.Failure($"Cannot update graph: Edge from '{srcNode.Name}' to '{targetNode.Name}' is already visited.")
            : Result<NarrativeEdge>.Success(edge);
    }

    private async Task<Result<ToolData.EdgeConditions>> CheckEdgeConditions(NarrativeEdge edge, CancellationToken ct = default)
    {
        var llm = models.CreateChat(EdgeCheckTemperature, nameof(UpdateGraphTool));
        var previousAttempts = new StringBuilder();

        for (int i = 0; i < EdgeCheckMaxRetries; i++)
        {
            string prompt = new PromptTemplate(instructions.Get(InstructionKey.CheckGraphUpdateConditions)).Render(
                new Dictionary<string, string>
                {
                    ["gameSummary"] = gameSummary,
                    ["ruling"] = ActionRulingFormatter.Format(ruling),
                    ["graph"] = NarrativeGraphFormatter.Format(campaign.NarrativeGraph!),
                    ["edge"] = NarrativeGraphFormatter.Format(edge),
                    ["attempts"] = previousAttempts.ToString()
                });

            var response = await llm.GenerateAsync(ChatRequest.ToChatRequest(prompt), cancellationToken: ct);
            string answer = response.LastMessageContent;

            if (!parser.TryParse<ToolData.EdgeConditions>(answer, out var toolData, out string? error))
            {
                previousAttempts.AppendLine($"Response: {answer}\nFailure reason: Invalid input syntax ({error})");
                continue;
            }
            var edgeConditions = toolData!;

            if (edgeConditions.Conditions is null)
            {
                previousAttempts.AppendLine($"Response: {answer}\nFailure reason: No edge conditions defined.");
                continue;
            }

            if (edgeConditions.Conditions.Count != edge.Conditions.Count)
            {
                previousAttempts.AppendLine(
                    $"Response: {answer}\nFailure reason: The number of conditions in the response is not equal to the number of conditions on the edge (response: {edgeConditions.Conditions.Count}, edge: {edge.Conditions.Count}).");
                continue;
            }

            if (edgeConditions.Conditions.Any(result => edge.Conditions.All(condition => result.Key != condition)))
            {
                previousAttempts.AppendLine(
                    $"Response: {answer}\nFailure reason: The response contains a condition not present in the list of conditions on the edge.");
                continue;
            }

            return Result<ToolData.EdgeConditions>.Success(edgeConditions);
        }
        return Result<ToolData.EdgeConditions>.Failure();
    }
}
