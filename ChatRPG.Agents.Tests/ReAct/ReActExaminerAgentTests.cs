using ChatRPG.Agents.ReAct;

namespace ChatRPG.Agents.Tests.ReAct;

public class ReActExaminerAgentTests
{
    [Theory]
    [InlineData("Verdict: ALLOWED", true)]
    [InlineData("Verdict: CONDITIONALLY ALLOWED", true)]
    [InlineData("Verdict: DISALLOWED", false)]
    public void ParseVerdict_ReadsWhetherThePlayerMayAct(string verdict, bool expectedIsAllowed)
    {
        var parsed = ReActExaminerAgent.ParseVerdict($"{verdict}\nThe gate is locked.");

        Assert.Equal(expectedIsAllowed, parsed.IsAllowed);
    }

    [Fact]
    public void ParseVerdict_KeepsOnlyTheReasoning()
    {
        // The narrator renders this as "Denied (reasoning: ...)", so the verdict line must not repeat.
        var parsed = ReActExaminerAgent.ParseVerdict(
            """
            Verdict: DISALLOWED
            The Ornate Crypt Key is still in the reliquary, so the door will not open.
            """);

        Assert.Equal("The Ornate Crypt Key is still in the reliquary, so the door will not open.", parsed.Reasoning);
    }

    [Fact]
    public void ParseVerdict_VerdictAndReasoningOnOneLine_KeepsTheWholeAnswer()
    {
        var parsed = ReActExaminerAgent.ParseVerdict("Verdict: ALLOWED - the door is already unlocked.");

        Assert.Equal("Verdict: ALLOWED - the door is already unlocked.", parsed.Reasoning);
    }
}
