using ChatRPG.Agents.ReAct;

namespace ChatRPG.Agents.Tests.ReAct;

public class FinalAnswerFilterTests
{
    [Fact]
    public void Consume_WithholdsEverythingBeforeTheMarker()
    {
        var filter = new FinalAnswerFilter();

        Assert.Null(filter.Consume("Thought: Do I need to use a tool? "));
        Assert.Null(filter.Consume("No\n"));
    }

    [Fact]
    public void Consume_MarkerSplitAcrossDeltas_IsStillFound()
    {
        // The model streams a token at a time, so the marker rarely arrives in one piece.
        var filter = new FinalAnswerFilter();

        Assert.Null(filter.Consume("Final"));
        Assert.Null(filter.Consume(" Answer"));
        Assert.Equal("The door", filter.Consume(": The door"));
    }

    [Fact]
    public void Consume_DropsTheSpaceTheModelWritesAfterTheMarker()
    {
        var filter = new FinalAnswerFilter();

        Assert.Equal("The door creaks open.", filter.Consume("Final Answer: The door creaks open."));
    }

    [Fact]
    public void Consume_PastTheMarker_PassesEveryDeltaThrough()
    {
        var filter = new FinalAnswerFilter();
        filter.Consume("Final Answer:");

        // Including whitespace-only ones, which carry the narration's line breaks.
        Assert.Equal("The door", filter.Consume("The door"));
        Assert.Equal(" ", filter.Consume(" "));
        Assert.Equal("creaks open.", filter.Consume("creaks open."));
    }

    [Fact]
    public void Consume_AToolStep_NeverEmits()
    {
        // Nothing of a tool call is the player's to read.
        var filter = new FinalAnswerFilter();

        foreach (string delta in new[] { "Action", ": search", "tool\nAction", " Input: ", "{\"query\": \"hall\"}" })
        {
            Assert.Null(filter.Consume(delta));
        }
    }
}
