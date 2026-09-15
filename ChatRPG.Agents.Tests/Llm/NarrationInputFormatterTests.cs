using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Tests.ReAct;
using ChatRPG.Application.Gameplay;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tests.Llm;

/// <summary>
/// What the narrator is actually handed as its input. An open-world turn is the player's words and
/// nothing else; a scenario turn carries what the examiner and navigator worked out along with it.
/// </summary>
public class NarrationInputFormatterTests
{
    [Fact]
    public void Format_OpenWorldTurn_PassesThePlayerThrough()
    {
        var request = new NarrationRequest.PlayerTurn(
            NarratorFixture.Campaign(isOpenWorld: true), new PlayerAction(PlayerActionKind.Do, "I open the door"));

        // No scaffolding to add: there is no ruling and no graph behind an open world.
        Assert.Equal("I open the door", NarrationInputFormatter.Format(request));
    }

    [Fact]
    public void Format_ScenarioTurn_CarriesTheRulingAndTheGraphUpdate()
    {
        var campaign = NarratorFixture.Campaign(isOpenWorld: false, NarratorFixture.Graph());

        var request = new NarrationRequest.PlayerTurn(
            campaign,
            new PlayerAction(PlayerActionKind.Say, "I greet the innkeeper"),
            new ActionRuling(ActionPermission.Disallowed, "The innkeeper has already left."),
            "Moved to The Tavern.");

        string formatted = NarrationInputFormatter.Format(request);

        Assert.Contains("Player input (Say):", formatted);
        Assert.Contains("I greet the innkeeper", formatted);
        Assert.Contains("Ruling: DISALLOWED", formatted);
        Assert.Contains("Reasoning: The innkeeper has already left.", formatted);
        Assert.Contains("Graph update summary:", formatted);
        Assert.Contains("Moved to The Tavern.", formatted);
    }

    [Fact]
    public void Format_ConditionalRuling_ReachesTheNarratorAsItsOwnState()
    {
        // The narrator branches on three states; "conditional" must not arrive looking like a denial.
        var campaign = NarratorFixture.Campaign(isOpenWorld: false, NarratorFixture.Graph());

        var request = new NarrationRequest.PlayerTurn(
            campaign,
            new PlayerAction(PlayerActionKind.Do, "I cross the rope bridge"),
            new ActionRuling(ActionPermission.Conditional, "The rope is frayed."));

        Assert.Contains("Ruling: CONDITIONAL", NarrationInputFormatter.Format(request));
    }

    [Fact]
    public void Format_ScenarioTurnWithoutAGraphUpdate_LeavesTheSectionOut()
    {
        var campaign = NarratorFixture.Campaign(isOpenWorld: false, NarratorFixture.Graph());

        var request = new NarrationRequest.PlayerTurn(
            campaign,
            new PlayerAction(PlayerActionKind.Do, "I wait"),
            new ActionRuling(ActionPermission.Allowed, "Nothing stops it."));

        string formatted = NarrationInputFormatter.Format(request);

        Assert.Contains("Ruling: ALLOWED", formatted);
        Assert.DoesNotContain("Graph update summary:", formatted);
    }

    [Fact]
    public void Format_Opening_LeadsWithTheScenario()
    {
        var request = new NarrationRequest.Opening(
            NarratorFixture.Campaign(isOpenWorld: true), "You wake in a tavern.");

        string formatted = NarrationInputFormatter.Format(request);

        Assert.StartsWith("Opening:", formatted);
        Assert.Contains("You wake in a tavern.", formatted);
    }

    [Fact]
    public void Format_Epilogue_ShowsBothHalvesOfTheTurnThatEndedTheCampaign()
    {
        // The closing scene should speak to what the player did, not only to the narration it provoked.
        var request = new NarrationRequest.Epilogue(
            NarratorFixture.Campaign(isOpenWorld: true),
            new PlayerAction(PlayerActionKind.Do, "I drink the vial"),
            "Aldric collapses.");

        Assert.Equal("Player: I drink the vial\nGM: Aldric collapses.", NarrationInputFormatter.Format(request));
    }
}
