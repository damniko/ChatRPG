using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Application.Gameplay;
using ChatRPG.Domain.Entities;
using Moq;

namespace ChatRPG.Agents.Tests.ReAct;

/// <summary>
/// The narrator's job is to assemble the right agent for the request: which prompt it speaks from,
/// which instruction it is given, and what it may reach for. Those choices are what these pin down.
/// </summary>
public class ReActNarratorAgentTests
{
    private const string Answer = "Final Answer: The tavern door swings open.";

    [Fact]
    public async Task NarrateStreamingAsync_OpenWorld_SpeaksFromTheOpenWorldPromptWithoutTheScenarioTool()
    {
        var fixture = NarratorFixture.Streaming(null, Answer);
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);

        await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(Turn(campaign)));

        string prompt = fixture.FirstPrompt();

        Assert.StartsWith("OPEN WORLD", prompt);
        Assert.Contains("woundcharactertool", prompt);
        Assert.Contains("healcharactertool", prompt);
        Assert.Contains("battletool", prompt);

        // There is no scenario document to search in an open world, so the tool is never built.
        Assert.DoesNotContain("searchscenariotool", prompt);
        fixture.Tools.Verify(t => t.GetSearchScenarioTool(It.IsAny<Campaign>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NarrateStreamingAsync_ScenarioCampaign_BindsTheGraphAndAddsTheScenarioTool()
    {
        var fixture = NarratorFixture.Streaming(null, Answer);
        var graph = NarratorFixture.Graph();
        var campaign = NarratorFixture.Campaign(isOpenWorld: false, graph);

        await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(Turn(campaign)));

        string prompt = fixture.FirstPrompt();

        Assert.StartsWith("SCENARIO", prompt);
        Assert.Contains(NarrativeGraphFormatter.Format(graph), prompt);
        Assert.Contains("searchscenariotool", prompt);
    }

    [Fact]
    public async Task NarrateStreamingAsync_ScenarioCampaignWithoutAGraph_Refuses()
    {
        var fixture = NarratorFixture.Streaming(null, Answer);
        var campaign = NarratorFixture.Campaign(isOpenWorld: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(Turn(campaign))));

        Assert.Contains("narrative graph", exception.Message);
    }

    [Fact]
    public async Task NarrateStreamingAsync_StreamsOnlyWhatFollowsTheFinalAnswerMarker()
    {
        var fixture = NarratorFixture.Streaming(
            null, "Thought: Do I need to use a tool? No\nFinal Answer: The tavern door swings open.");
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);

        var chunks = await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(Turn(campaign)));

        Assert.Equal("The tavern door swings open.", string.Concat(chunks));
        Assert.True(chunks.Count > 1, "The narration should reach the player as it is written.");
        fixture.Models.Verify(m => m.CreateChat(It.IsAny<double>(), It.IsAny<string?>(), true));
    }

    [Fact]
    public async Task NarrateStreamingAsync_WhenStreamingIsTurnedOff_YieldsTheNarrationAsOneChunk()
    {
        var options = new AgentOptions { StreamChatCompletions = false };
        var fixture = NarratorFixture.Answering(options, Answer);
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);

        var chunks = await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(Turn(campaign)));

        Assert.Equal(["The tavern door swings open."], chunks);

        // Asking a non-streaming model to stream would hand the player the whole block at the end anyway.
        fixture.Models.Verify(m => m.CreateChat(It.IsAny<double>(), It.IsAny<string?>(), true), Times.Never);
    }

    [Theory]
    [InlineData(PlayerActionKind.Do, "<instruction:Do>")]
    [InlineData(PlayerActionKind.Say, "<instruction:Say>")]
    public async Task NarrateStreamingAsync_OpenWorldTurn_IsGivenTheInstructionForTheActionKind(
        PlayerActionKind kind, string expectedInstruction)
    {
        var fixture = NarratorFixture.Streaming(null, Answer);
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);

        var request = new NarrationRequest.PlayerTurn(campaign, new PlayerAction(kind, "I open the door"));
        await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(request));

        Assert.Contains($"Action: {expectedInstruction}", fixture.FirstPrompt());
    }

    [Fact]
    public async Task NarrateAsync_Epilogue_UsesTheGameOverInstructionAndShowsTheTurnThatEndedIt()
    {
        var fixture = NarratorFixture.Answering(null, "Final Answer: And so the tale closes.");
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);

        var request = new NarrationRequest.Epilogue(
            campaign, new PlayerAction(PlayerActionKind.Do, "I drink the vial"), "Aldric collapses.");

        string epilogue = await fixture.Narrator().NarrateAsync(request);

        Assert.Equal("And so the tale closes.", epilogue);

        string prompt = fixture.FirstPrompt();
        Assert.Contains($"Action: <instruction:{InstructionKey.GameOver}>", prompt);
        Assert.Contains("Player: I drink the vial", prompt);
        Assert.Contains("GM: Aldric collapses.", prompt);
    }

    [Fact]
    public async Task NarrateStreamingAsync_Opening_IsGivenTheInitialInstructionAndTheScenario()
    {
        var fixture = NarratorFixture.Streaming(null, Answer);
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);

        var request = new NarrationRequest.Opening(campaign, "You wake in a tavern.");
        await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(request));

        string prompt = fixture.FirstPrompt();
        Assert.Contains($"Action: <instruction:{InstructionKey.Initial}>", prompt);
        Assert.Contains("You wake in a tavern.", prompt);
    }

    [Fact]
    public async Task NarrateStreamingAsync_CarriesTheRunningSummaryIntoThePrompt()
    {
        var fixture = NarratorFixture.Streaming(null, Answer);
        var campaign = NarratorFixture.Campaign(isOpenWorld: true);
        campaign.GameSummary = "Aldric has been travelling for three days.";

        await ChatModelStub.Collect(fixture.Narrator().NarrateStreamingAsync(Turn(campaign)));

        Assert.Contains("Aldric has been travelling for three days.", fixture.FirstPrompt());
    }

    private static NarrationRequest Turn(Campaign campaign)
    {
        return new NarrationRequest.PlayerTurn(campaign, new PlayerAction(PlayerActionKind.Do, "I open the door"));
    }
}
