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

internal sealed class SearchScenarioTool(
    Campaign campaign,
    string gameSummary,
    IScenarioDocumentStore documents,
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IToolDescriptionCatalog descriptions,
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.SearchScenario> validator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.SearchScenario))
{
    private const string ToolName = "searchscenariotool";
    private const int ExcerptsPerQuery = 10;
    private const double DistillationTemperature = 0.1;

    public override async Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.SearchScenario>(input, out var toolData, out string? error))
        {
            return $"Invalid input syntax: {error}";
        }
        var search = toolData!;

        if (!validator.IsValid(search, out var errors))
        {
            return $"Validation error: {string.Join(", ", errors)}";
        }

        NarrativeNode? node = null;
        if (search.NodeName is not null)
        {
            node = campaign.NarrativeGraph!.Nodes.FirstOrDefault(n => n.Name == search.NodeName);
            if (node is null)
            {
                return $"Node with name {search.NodeName} not found.";
            }
        }

        string prompt = new PromptTemplate(instructions.Get(InstructionKey.SearchScenario)).Render(
            new Dictionary<string, string>
            {
                ["gameSummary"] = gameSummary,
                // TODO: Normalize variable usages in templates (e.g., with/without headers?)
                ["graph"] = $"Scenario Graph:\n{campaign.NarrativeGraph!.Serialize()}\n",
                ["context"] = await GatherContextAsync(search.Query!, node, ct),
                ["input"] = input,
            });

        var response = await models.CreateChat(DistillationTemperature)
            .GenerateAsync(ChatRequest.ToChatRequest(prompt), settings: null, ct);

        return response.LastMessageContent;
    }

    /// <summary>
    /// Gathers the excerpts to reason over: the query itself, and - when the agent named a node -
    /// that node plus the discovered nodes leading into it, so the answer is anchored in the part of
    /// the graph the player can actually have reached.
    /// </summary>
    private async Task<string> GatherContextAsync(string query, NarrativeNode? node, CancellationToken ct)
    {
        var context = new StringBuilder()
            .Append("Context for input: ").Append(await SearchAsync(query, ct)).Append('\n');

        if (node is null)
        {
            return context.ToString();
        }

        string nodeQuery = DescribeForSearch(node, node.Edges.Where(e => e.EdgeStatus == NarrativeEdge.Status.Unvisited));
        context.Append($"Context for node {node.Name}: ").Append(await SearchAsync(nodeQuery, ct)).Append('\n');

        var reachableParents = campaign.NarrativeGraph!.GetIncomingNodes(node)
            .Where(n => n.NodeStatus != NarrativeNode.Status.Undiscovered);

        foreach (var parent in reachableParents)
        {
            string parentQuery = DescribeForSearch(parent, parent.Edges.Where(e => e.TargetNode == node));
            context.Append($"Context for node {parent.Name}: ").Append(await SearchAsync(parentQuery, ct)).Append('\n');
        }

        return context.ToString();
    }

    private async Task<string> SearchAsync(string query, CancellationToken ct)
    {
        var excerpts = await documents.SearchAsync(campaign.Id, query, ExcerptsPerQuery, ct);

        return string.Join("\n", excerpts.Select(excerpt => excerpt.Content));
    }

    private static string DescribeForSearch(NarrativeNode node, IEnumerable<NarrativeEdge> edges)
    {
        return $"{node.Name} {node.StoryContent} {string.Join(" ", edges.SelectMany(e => e.Conditions))}";
    }
}
