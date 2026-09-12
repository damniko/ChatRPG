using ChatRPG.Agents.ReAct;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tests.ReAct;

public class ReActToolboxTests
{
    [Fact]
    public void RendersTheNamesAndDescriptionsThePromptsAskFor()
    {
        var toolbox = new ReActToolbox([
            new StubTool("searchscenariotool", "Looks things up."),
            new StubTool("battletool", "Resolves a fight."),
        ]);

        Assert.Equal("searchscenariotool,battletool", toolbox.Names);
        Assert.Equal("searchscenariotool, Looks things up.\nbattletool, Resolves a fight.", toolbox.Descriptions);
    }

    [Fact]
    public async Task ObserveAsync_RunsTheNamedTool_IgnoringCase()
    {
        var tool = new StubTool("searchscenariotool", "Looks things up.");
        var toolbox = new ReActToolbox([tool]);

        string observation = await toolbox.ObserveAsync("SearchScenarioTool", "the input");

        Assert.Equal("ran with: the input", observation);
    }

    [Fact]
    public async Task ObserveAsync_UnknownTool_ObservesTheRealOnesInsteadOfThrowing()
    {
        var toolbox = new ReActToolbox([new StubTool("searchscenariotool", "Looks things up.")]);

        string observation = await toolbox.ObserveAsync("inventedtool", "the input");

        Assert.Contains("no tool named \"inventedtool\"", observation);
        Assert.Contains("searchscenariotool", observation);
    }

    private sealed class StubTool(string name, string description) : AgentTool(name, description)
    {
        public override Task<string> ToolTask(string input, CancellationToken token = default)
        {
            return Task.FromResult($"ran with: {input}");
        }
    }
}
