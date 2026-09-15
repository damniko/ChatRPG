using ChatRPG.Agents.ReAct;
using ChatRPG.Agents.ReAct.Agents;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tests.ReAct;

public class ReActExaminerAgentTests
{
    [Theory]
    [InlineData("Ruling: ALLOWED", ActionPermission.Allowed)]
    [InlineData("Ruling: CONDITIONAL", ActionPermission.Conditional)]
    [InlineData("Ruling: DISALLOWED", ActionPermission.Disallowed)]
    public void ParseRuling_ReadsWhetherThePlayerMayAct(string ruling, ActionPermission expected)
    {
        var parsed = ReActExaminerAgent.ParseRuling($"{ruling}\nThe gate is locked.");

        Assert.Equal(expected, parsed.Permission);
    }

    [Fact]
    public void ParseRuling_ModelWroteTheOldConditionallyAllowedWording_IsStillConditional()
    {
        var parsed = ReActExaminerAgent.ParseRuling("Ruling: CONDITIONALLY ALLOWED\nThe rope is frayed.");

        Assert.Equal(ActionPermission.Conditional, parsed.Permission);
        Assert.Equal("The rope is frayed.", parsed.Reasoning);
    }

    [Fact]
    public void ParseRuling_KeepsOnlyTheReasoning()
    {
        var parsed = ReActExaminerAgent.ParseRuling(
            """
            Ruling: DISALLOWED
            The Ornate Crypt Key is still in the reliquary, so the door will not open.
            """);

        Assert.Equal("The Ornate Crypt Key is still in the reliquary, so the door will not open.", parsed.Reasoning);
    }

    [Fact]
    public void ParseRuling_ReasoningMentionsDisallowed_DoesNotFlipThePermission()
    {
        var parsed = ReActExaminerAgent.ParseRuling(
            """
            Ruling: ALLOWED
            Entering the vault would be disallowed, but the antechamber is open to anyone.
            """);

        Assert.Equal(ActionPermission.Allowed, parsed.Permission);
    }

    [Fact]
    public void ParseRuling_RulingAndReasoningOnOneLine_SplitsThemAnyway()
    {
        var parsed = ReActExaminerAgent.ParseRuling("Ruling: ALLOWED - the door is already unlocked.");

        Assert.Equal(ActionPermission.Allowed, parsed.Permission);
        Assert.Equal("the door is already unlocked.", parsed.Reasoning);
    }

    [Fact]
    public void ParseRuling_NoMarker_AllowsAndKeepsTheWholeAnswerAsReasoning()
    {
        var parsed = ReActExaminerAgent.ParseRuling("The door is already unlocked.");

        Assert.Equal(ActionPermission.Allowed, parsed.Permission);
        Assert.Equal("The door is already unlocked.", parsed.Reasoning);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ruling: ALLOWED")]
    public void ParseRuling_NoReasoning_StillProducesReasoning(string answer)
    {
        var parsed = ReActExaminerAgent.ParseRuling(answer);

        Assert.False(string.IsNullOrWhiteSpace(parsed.Reasoning));
    }
}
