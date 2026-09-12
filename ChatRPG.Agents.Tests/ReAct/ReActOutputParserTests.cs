using ChatRPG.Agents.ReAct;

namespace ChatRPG.Agents.Tests.ReAct;

public class ReActOutputParserTests
{
    [Fact]
    public void Parse_FinalAnswer_ReturnsTextAfterTheMarker()
    {
        const string output = """
                              Thought: Do I need to use a tool? No
                              Final Answer: Verdict: ALLOWED
                              The door is already unlocked.
                              """;

        var step = Assert.IsType<ReActStep.FinalAnswer>(ReActOutputParser.Parse(output));

        Assert.Equal("Verdict: ALLOWED\nThe door is already unlocked.", step.Text);
    }

    [Fact]
    public void Parse_Action_ReturnsToolNameAndInput()
    {
        // Generation stops at "Observation", so the action input runs to the end of the output.
        const string output = """
                              Thought: Do I need to use a tool? Yes
                              Action: searchscenariotool
                              Action Input: {"query": "What is in the hall?", "nodename": "Castle Hall"}
                              """;

        var step = Assert.IsType<ReActStep.UseTool>(ReActOutputParser.Parse(output));

        Assert.Equal("searchscenariotool", step.Name);
        Assert.Equal("""{"query": "What is in the hall?", "nodename": "Castle Hall"}""", step.Input);
    }

    [Fact]
    public void Parse_MultiLineActionInput_KeepsTheWholeInput()
    {
        const string output = """
                              Action: updategraphtool
                              Action Input: {
                                  "sourcenodename": "Ruined Bridge",
                                  "targetnodename": "Other Side"
                              }
                              """;

        var step = Assert.IsType<ReActStep.UseTool>(ReActOutputParser.Parse(output));

        Assert.Contains("\"targetnodename\": \"Other Side\"", step.Input);
    }

    [Fact]
    public void Parse_BothAnswerAndAction_PrefersTheAnswer()
    {
        // A model that writes both has stopped acting; running the tool anyway would loop.
        const string output = """
                              Action: searchscenariotool
                              Action Input: {"query": "anything"}
                              Final Answer: Verdict: DISALLOWED
                              """;

        var step = Assert.IsType<ReActStep.FinalAnswer>(ReActOutputParser.Parse(output));

        Assert.Equal("Verdict: DISALLOWED", step.Text);
    }

    [Theory]
    [InlineData("Thought: I am not sure what to do here.")]
    [InlineData("Action Input: {\"query\": \"no action named\"}")]
    [InlineData("Action:\nAction Input: {}")]
    public void Parse_WithoutAUsableStep_IsUnparsable(string output)
    {
        Assert.IsType<ReActStep.Unparsable>(ReActOutputParser.Parse(output));
    }
}
