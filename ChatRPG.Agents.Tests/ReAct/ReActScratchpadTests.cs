using ChatRPG.Agents.ReAct;

namespace ChatRPG.Agents.Tests.ReAct;

public class ReActScratchpadTests
{
    [Fact]
    public void ToString_BeforeAnySteps_IsEmpty()
    {
        Assert.Equal(string.Empty, new ReActScratchpad().ToString());
    }

    [Fact]
    public void Record_LaysOutTheStepAsTheModelWroteIt()
    {
        var scratchpad = new ReActScratchpad();

        scratchpad.Record("Action: sometool\nAction Input: {}", "It worked.");

        Assert.Equal(
            """
            Action: sometool
            Action Input: {}
            Observation: It worked.
            Thought:
            """,
            scratchpad.ToString());
    }

    [Fact]
    public void Record_SecondStep_AppendsBelowTheFirst()
    {
        var scratchpad = new ReActScratchpad();

        scratchpad.Record("Action: first\nAction Input: {}", "one");
        scratchpad.Record("Action: second\nAction Input: {}", "two");

        Assert.Equal(
            """
            Action: first
            Action Input: {}
            Observation: one
            Thought:
            Action: second
            Action Input: {}
            Observation: two
            Thought:
            """,
            scratchpad.ToString());
    }
}
