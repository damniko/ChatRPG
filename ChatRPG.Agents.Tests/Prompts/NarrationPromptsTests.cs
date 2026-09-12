using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Tests.ReAct;
using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Gameplay;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tests.Prompts;

/// <summary>
/// The turn pipeline states what happened; this is where that becomes a choice of wording. Pinning
/// the mapping keeps the Application layer free of prompt names.
/// </summary>
public class NarrationPromptsTests
{
    [Theory]
    [InlineData(true, nameof(InstructionKey.Narrate))]
    [InlineData(false, nameof(InstructionKey.NarrateWithGraph))]
    public void SystemPromptFor_FollowsWhetherTheCampaignHasAScenario(bool isOpenWorld, string expected)
    {
        var campaign = NarratorFixture.Campaign(isOpenWorld, isOpenWorld ? null : NarratorFixture.Graph());

        var request = new NarrationRequest.PlayerTurn(campaign, Action(PlayerActionKind.Do));

        Assert.Equal(expected, NarrationPrompts.SystemPromptFor(request).ToString());
    }

    [Fact]
    public void InstructionFor_Opening_OpensTheStory()
    {
        var request = new NarrationRequest.Opening(OpenWorld(), "You wake in a tavern.");

        Assert.Equal(InstructionKey.Initial, NarrationPrompts.InstructionFor(request));
    }

    [Fact]
    public void InstructionFor_Epilogue_ClosesIt()
    {
        var request = new NarrationRequest.Epilogue(OpenWorld(), Action(PlayerActionKind.Do), "Aldric collapses.");

        Assert.Equal(InstructionKey.GameOver, NarrationPrompts.InstructionFor(request));
    }

    [Theory]
    [InlineData(PlayerActionKind.Do, false, nameof(InstructionKey.Do))]
    [InlineData(PlayerActionKind.Do, true, nameof(InstructionKey.DoWithVerdict))]
    [InlineData(PlayerActionKind.Say, false, nameof(InstructionKey.Say))]
    [InlineData(PlayerActionKind.Say, true, nameof(InstructionKey.SayWithVerdict))]
    public void InstructionFor_PlayerTurn_FollowsTheActionKindAndWhetherTheExaminerRuled(
        PlayerActionKind kind, bool hasVerdict, string expected)
    {
        var verdict = hasVerdict ? new AdherenceVerdict(true, "The door is unlocked.") : null;

        var request = new NarrationRequest.PlayerTurn(OpenWorld(), Action(kind), verdict);

        Assert.Equal(expected, NarrationPrompts.InstructionFor(request).ToString());
    }

    private static Campaign OpenWorld() => NarratorFixture.Campaign(isOpenWorld: true);

    private static PlayerAction Action(PlayerActionKind kind) => new(kind, "I open the door");
}
