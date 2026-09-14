using ChatRPG.Agents.Llm;
using ChatRPG.Agents.ReAct;
using LangChain.Chains.StackableChains.Agents.Tools;
using LangChain.Providers;
using Moq;

namespace ChatRPG.Agents.Tests.Llm;

/// <summary>
/// Tracking lives in the model the factory hands out rather than in the agent, so these check that a
/// ReAct run is billed for every step it takes and not just the one that answers.
/// </summary>
public class ReActAgentUsageTests
{
    private const string Prompt =
        "Tools: {tools} [{tool_names}]\nNew input: {input}\nPrevious tool steps: {history}";

    private readonly RecordingUsageSink _sink = new();

    [Fact]
    public async Task RunAsync_ToolSteps_RecordsUsageForEveryModelCall()
    {
        var model = Track(ChatModelStub.Answering(
            "Action: sometool\nAction Input: {\"query\": \"the hall\"}",
            "Action: sometool\nAction Input: {\"query\": \"the throne\"}",
            "Thought: Do I need to use a tool? No\nFinal Answer: A suit of armour stands by the throne."));

        var agent = new ReActAgent(model, Prompt)
        {
            Tools = { new StubTool("sometool", "Looks things up.") },
        };

        await agent.RunAsync("I look around");

        // Two tool steps plus the answering step: three calls, three bills.
        Assert.Equal(3, _sink.Calls.Count);
        Assert.All(_sink.Calls, call => Assert.Equal("Narrator", call.Operation));
    }

    [Fact]
    public async Task RunStreamingAsync_FinalAnswer_RecordsUsageDespiteAbandoningTheStream()
    {
        // The streaming loop stops enumerating the moment it has the final answer. If usage were
        // recorded after the loop instead of on disposal, this would record nothing.
        var model = Track(ChatModelStub.Streaming(
            "Thought: Do I need to use a tool? No\nFinal Answer: The door creaks open."));

        var agent = new ReActAgent(model, Prompt);

        await ChatModelStub.Collect(agent.RunStreamingAsync("I open the door"));

        Assert.Single(_sink.Calls);
    }

    private UsageTrackingChatModel Track(Mock<IChatModel> model)
    {
        model.SetupGet(m => m.Id).Returns("gpt-4o");
        model.SetupGet(m => m.ContextLength).Returns(128_000);

        return new UsageTrackingChatModel(model.Object, "Narrator", _sink, TimeProvider.System);
    }

    private sealed class StubTool(string name, string description) : AgentTool(name, description)
    {
        public override Task<string> ToolTask(string input, CancellationToken token = default)
        {
            return Task.FromResult($"looked up: {input}");
        }
    }
}
