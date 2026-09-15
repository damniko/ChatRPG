using System.Runtime.CompilerServices;
using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Gameplay;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.ReAct;

/// <summary>
/// Tells the player what happens. It is the only agent that writes for the player to read, so it is
/// also the only one that streams: the narration appears as it is written rather than in one block.
/// </summary>
internal sealed class ReActNarratorAgent(
    IChatModelFactory models,
    GameSummaryFormatter summaryFormatter,
    IInstructionCatalog instructions,
    IToolFactory tools,
    IOptions<AgentOptions> options) : INarrator
{
    private const double Temperature = 0.7;

    public IAsyncEnumerable<string> NarrateStreamingAsync(NarrationRequest request, CancellationToken ct = default)
    {
        if (!options.Value.StreamChatCompletions)
        {
            return AsSingleChunkAsync(request, ct);
        }

        return StreamAsync(request, ct);
    }

    private async IAsyncEnumerable<string> StreamAsync(
        NarrationRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        var agent = await CreateAgentAsync(request, streaming: true, ct);

        await foreach (string chunk in agent.RunStreamingAsync(NarrationInputFormatter.Format(request), ct))
        {
            yield return chunk;
        }
    }

    public async Task<string> NarrateAsync(NarrationRequest request, CancellationToken ct = default)
    {
        var agent = await CreateAgentAsync(request, streaming: false, ct);
        return await agent.RunAsync(NarrationInputFormatter.Format(request), ct);
    }

    private async IAsyncEnumerable<string> AsSingleChunkAsync(NarrationRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        yield return await NarrateAsync(request, ct);
    }

    /// <summary>
    /// Builds the narrator for one request. Which prompt it speaks from, what it is told to do, and what
    /// it may reach for all depend on whether the campaign follows a scenario or is open world.
    /// </summary>
    private async Task<ReActAgent> CreateAgentAsync(NarrationRequest request, bool streaming, CancellationToken ct)
    {
        var campaign = request.Campaign;

        var agent = new ReActAgent(
            models.CreateChat(Temperature, nameof(ReActNarratorAgent), streaming),
            instructions.Get(NarrationPrompts.SystemPromptFor(request)))
        {
            Variables =
            {
                ["gameSummary"] = await summaryFormatter.FormatAsync(campaign.Id, campaign.GameSummary, ct),
                ["action"] = instructions.Get(NarrationPrompts.InstructionFor(request)),
            },
            Tools =
            {
                tools.GetWoundCharacterTool(campaign),
                tools.GetHealCharacterTool(campaign),
                tools.GetBattleTool(campaign),
            },
        };

        if (campaign.IsOpenWorld)
        {
            // The open-world prompt has no graph to bind, and there is no scenario document to search.
            return agent;
        }

        var graph = campaign.NarrativeGraph ?? throw new InvalidOperationException(
            "A campaign that is not open world must have a narrative graph to narrate from.");

        agent.Variables["graph"] = NarrativeGraphFormatter.Format(graph);
        agent.Tools.Add(tools.GetSearchScenarioTool(campaign, agent.Variables["gameSummary"]));

        return agent;
    }
}
