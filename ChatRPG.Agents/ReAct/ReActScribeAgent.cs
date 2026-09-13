using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.ReAct.Exceptions;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using LangChain.DocumentLoaders;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.ReAct;

// TODO: Handle debug in general (AgentOptions.DebugAgents)
internal sealed class ReActScribeAgent(
    IDocumentLoader documentLoader,
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IOptions<AgentOptions> options,
    IToolFactory toolFactory) : INarrativeGraphScribe
{
    private const double Temperature = 0.4;
    
    public async Task<NarrativeGraph> ScribeAsync(
        byte[] scenarioPdf,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        var graph = new NarrativeGraph();
        var startNode = graph.InitializeStartNode();

        var documents = await documentLoader.LoadAsync(DataSource.FromBytes(scenarioPdf), cancellationToken: ct);

        string prevGraphSummary = $"The starting node '{startNode.Name}' has been added as the beginning of the story.";

        for (int i = 0; i < documents.Count; i += options.Value.ScribeBatchSize)
        {
            var agent = new ReActAgent(models.CreateChat(Temperature, nameof(ReActScribeAgent)), instructions.Get(InstructionKey.Scribe))
            {
                Variables =
                {
                    ["graph"] = NarrativeGraphFormatter.Format(graph),
                    ["graphExtensionSummary"] = prevGraphSummary
                },
                Tools =
                {
                    toolFactory.GetAddNodeTool(graph),
                    toolFactory.GetAddEdgeTool(graph),
                    toolFactory.GetAddEndNodeTool(graph)
                }
            };

            int batchSize = options.Value.ScribeBatchSize;
            string pages = string.Join("\n", documents.Skip(i).Take(batchSize));
            if (i + batchSize >= documents.Count)
            {
                pages +=
                    "\n\nThis is the last page of the document. Make sure that the graph contains an end node before providing the final answer.";
            }

            try
            {
                prevGraphSummary = await agent.RunAsync(pages, ct);
            }
            catch (NoFinalAnswerReachedException)
            {
                prevGraphSummary = $"No summary generated for the previous graph extension session. Here is the last summary available: {prevGraphSummary}";
            }

            int progressValue = (int)((i + batchSize) / (double)documents.Count * 100);
            progress?.Report(Math.Min(progressValue, 100));
        }

        return graph;
    }
}
