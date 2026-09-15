using ChatRPG.Agents.ReAct;
using ChatRPG.Agents.ReAct.Agents;
using ChatRPG.Agents.ReAct.Exceptions;
using LangChain.Chains.StackableChains.Agents.Tools;
using LangChain.Providers;
using Moq;

namespace ChatRPG.Agents.Tests.ReAct;

public class ReActAgentTests
{
    private const string Prompt = "Tools: {tools} [{tool_names}]\nNew input: {input}\nPrevious tool steps: {history}";

    [Fact]
    public async Task RunAsync_FinalAnswerOnTheFirstPass_ReturnsIt()
    {
        var model = ChatModelStub.Answering("Thought: Do I need to use a tool? No\nFinal Answer: Verdict: ALLOWED");

        string answer = await new ReActAgent(model.Object, Prompt).RunAsync("I open the door");

        Assert.Equal("Verdict: ALLOWED", answer);
    }

    [Fact]
    public async Task RunAsync_ToolStep_FeedsTheObservationBackAndAnswers()
    {
        var model = ChatModelStub.Answering(
            "Action: sometool\nAction Input: {\"query\": \"the hall\"}",
            "Thought: Do I need to use a tool? No\nFinal Answer: A suit of armour stands by the throne.");

        var agent = new ReActAgent(model.Object, Prompt)
        {
            Tools = { new StubTool("sometool", "Looks things up.") },
        };

        string answer = await agent.RunAsync("I look around");

        Assert.Equal("A suit of armour stands by the throne.", answer);

        // The second prompt must carry what the tool said, or the model is reasoning blind.
        string secondPrompt = ChatModelStub.PromptsSentTo(model)[1];
        Assert.Contains("Observation: looked up: {\"query\": \"the hall\"}", secondPrompt);
        Assert.Contains("Tools: sometool, Looks things up. [sometool]", secondPrompt);
        Assert.Contains("New input: I look around", secondPrompt);
    }

    [Fact]
    public async Task RunAsync_UnparsableOutput_AsksForTheFormatAgain()
    {
        var model = ChatModelStub.Answering(
            "I think the player should probably be allowed to do that.",
            "Final Answer: Verdict: ALLOWED");

        string answer = await new ReActAgent(model.Object, Prompt).RunAsync("I open the door");

        Assert.Equal("Verdict: ALLOWED", answer);
        Assert.Contains("did not follow the required format", ChatModelStub.PromptsSentTo(model)[1]);
    }

    [Fact]
    public async Task RunAsync_NeverAnswering_ThrowsOnceTheBudgetIsSpent()
    {
        var model = ChatModelStub.Answering("Action: sometool\nAction Input: {}");

        var agent = new ReActAgent(model.Object, Prompt)
        {
            Tools = { new StubTool("sometool", "Looks things up.") },
            MaxSteps = 3,
        };

        var exception = await Assert.ThrowsAsync<NoFinalAnswerReachedException>(() => agent.RunAsync("I stall"));

        Assert.Equal(3, exception.Steps);
        Assert.Equal(3, ChatModelStub.PromptsSentTo(model).Count);
    }

    [Fact]
    public async Task RunAsync_StopsGenerationAtTheObservationBoundary()
    {
        // Anything the model writes past "Observation" would be an invented tool result.
        var model = ChatModelStub.Answering("Final Answer: done");

        await new ReActAgent(model.Object, Prompt).RunAsync("I open the door");

        model.Verify(m => m.GenerateAsync(
            It.IsAny<ChatRequest>(),
            It.Is<ChatSettings>(s => s.StopSequences!.Contains("Observation") && s.StopSequences!.Contains("[END]")),
            It.IsAny<CancellationToken>()));
    }


    [Fact]
    public async Task RunStreamingAsync_FinalAnswerOnTheFirstPass_StreamsOnlyWhatFollowsTheMarker()
    {
        var model = ChatModelStub.Streaming("Thought: Do I need to use a tool? No\nFinal Answer: The door creaks open.");

        var chunks = await ChatModelStub.Collect(new ReActAgent(model.Object, Prompt).RunStreamingAsync("I open the door"));

        // The reasoning is the agent's business; only the narration is the player's.
        Assert.Equal("The door creaks open.", string.Concat(chunks));
        Assert.True(chunks.Count > 1, "The answer should arrive in pieces, not as one block.");
    }

    [Fact]
    public async Task RunStreamingAsync_ToolStep_StreamsNothingOfItAndStillFeedsTheObservationBack()
    {
        var model = ChatModelStub.Streaming(
            "Action: sometool\nAction Input: {\"query\": \"the hall\"}",
            "Thought: Do I need to use a tool? No\nFinal Answer: A suit of armour stands by the throne.");

        var agent = new ReActAgent(model.Object, Prompt)
        {
            Tools = { new StubTool("sometool", "Looks things up.") },
        };

        var chunks = await ChatModelStub.Collect(agent.RunStreamingAsync("I look around"));

        Assert.Equal("A suit of armour stands by the throne.", string.Concat(chunks));
        Assert.Contains("Observation: looked up: {\"query\": \"the hall\"}", ChatModelStub.PromptsSentTo(model)[1]);
    }

    [Fact]
    public async Task RunStreamingAsync_NeverAnswering_ThrowsOnceTheBudgetIsSpent()
    {
        var model = ChatModelStub.Streaming("Action: sometool\nAction Input: {}");

        var agent = new ReActAgent(model.Object, Prompt)
        {
            Tools = { new StubTool("sometool", "Looks things up.") },
            MaxSteps = 3,
        };

        var exception = await Assert.ThrowsAsync<NoFinalAnswerReachedException>(
            async () => await ChatModelStub.Collect(agent.RunStreamingAsync("I stall")));

        Assert.Equal(3, exception.Steps);
        Assert.Equal(3, ChatModelStub.PromptsSentTo(model).Count);
    }

    private sealed class StubTool(string name, string description) : AgentTool(name, description)
    {
        public override Task<string> ToolTask(string input, CancellationToken token = default)
        {
            return Task.FromResult($"looked up: {input}");
        }
    }
}
