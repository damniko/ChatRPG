using ChatRPG.Agents.Prompts;

namespace ChatRPG.Agents.Tests.ReAct;

public class PromptTemplateTests
{
    [Fact]
    public void Render_SubstitutesEveryBoundPlaceholder()
    {
        var template = new PromptTemplate("Graph: {graph}\nNew input: {input}");

        string rendered = template.Render(new Dictionary<string, string>
        {
            ["graph"] = "a graph",
            ["input"] = "I open the door",
        });

        Assert.Equal("Graph: a graph\nNew input: I open the door", rendered);
    }

    [Fact]
    public void Render_LeavesLiteralBracesAlone()
    {
        // Tool descriptions are full of JSON examples and value lists that are not placeholders.
        const string description = """
                                   Input to this tool must be RAW JSON: {"query": "<the query>"}
                                   Accepted severity values: {low, medium, high}
                                   """;

        var template = new PromptTemplate($"{{tools}}\n{description}");

        string rendered = template.Render(new Dictionary<string, string> { ["tools"] = "searchscenariotool" });

        Assert.Equal($"searchscenariotool\n{description}", rendered);
    }

    [Fact]
    public void Render_DoesNotRescanSubstitutedValues()
    {
        // A tool description mentioning {input} must not swallow the player's input.
        var template = new PromptTemplate("{tools}\n{input}");

        string rendered = template.Render(new Dictionary<string, string>
        {
            ["tools"] = "sometool, pass the player's {input} verbatim",
            ["input"] = "I open the door",
        });

        Assert.Equal("sometool, pass the player's {input} verbatim\nI open the door", rendered);
    }

    [Fact]
    public void Render_VariableThePromptNeverAsksFor_Throws()
    {
        // The bug this guards: two variables that quietly shared one placeholder.
        var template = new PromptTemplate("Game summary: {gameSummary}");

        var exception = Assert.Throws<InvalidOperationException>(() => template.Render(
            new Dictionary<string, string>
            {
                ["gameSummary"] = "the story so far",
                ["graphSummary"] = "what the scribe just added",
            }));

        Assert.Contains("graphSummary", exception.Message);
    }
}
