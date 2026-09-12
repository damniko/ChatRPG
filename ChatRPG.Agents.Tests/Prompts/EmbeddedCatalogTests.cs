using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Catalogs;

namespace ChatRPG.Agents.Tests.Prompts;

/// <summary>
/// The catalogs load every prompt and description up front, so constructing them is the check that
/// each markdown file exists and is embedded by the csproj.
/// </summary>
public class EmbeddedCatalogTests
{
    [Fact]
    public void EveryPromptIsEmbedded()
    {
        var catalog = new EmbeddedInstructionCatalog();

        Assert.All(Enum.GetValues<InstructionKey>(), key => Assert.NotEmpty(catalog.Get(key)));
    }

    [Fact]
    public void EveryToolDescriptionIsEmbedded()
    {
        var catalog = new EmbeddedToolDescriptionCatalog();

        Assert.All(Enum.GetValues<ToolDescriptionKey>(), key => Assert.NotEmpty(catalog.Get(key)));
    }

    [Fact]
    public void ExaminePrompt_RendersWithExactlyTheVariablesTheExaminerBinds()
    {
        // Pins the contract between ReActExaminerAgent and Examine.md: rendering throws if the agent
        // binds a variable the prompt dropped, and a placeholder left behind shows up as literal text.
        var prompt = new PromptTemplate(new EmbeddedInstructionCatalog().Get(InstructionKey.Examine));

        string rendered = prompt.Render(new Dictionary<string, string>
        {
            // Bound by the examiner.
            ["graph"] = "<graph>",
            ["gameSummary"] = "<game summary>",
            // Bound by the loop for every agent.
            ["input"] = "<input>",
            ["history"] = "<history>",
            ["tools"] = "<tools>",
            ["tool_names"] = "<tool names>",
        });

        Assert.DoesNotContain("{graph}", rendered);
        Assert.DoesNotContain("{gameSummary}", rendered);
        Assert.DoesNotContain("{input}", rendered);
        Assert.DoesNotContain("{history}", rendered);
        Assert.DoesNotContain("{tools}", rendered);
        Assert.DoesNotContain("{tool_names}", rendered);
    }

    [Fact]
    public void NarratePrompt_RendersWithExactlyTheVariablesTheNarratorBindsInAnOpenWorld()
    {
        // The open-world prompt has no {graph}; binding one would throw, which is why the narrator
        // only binds it for scenario campaigns.
        var prompt = new PromptTemplate(new EmbeddedInstructionCatalog().Get(InstructionKey.Narrate));

        string rendered = prompt.Render(new Dictionary<string, string>
        {
            // Bound by the narrator.
            ["gameSummary"] = "<game summary>",
            ["action"] = "<action>",
            // Bound by the loop for every agent.
            ["input"] = "<input>",
            ["history"] = "<history>",
            ["tools"] = "<tools>",
            ["tool_names"] = "<tool names>",
        });

        Assert.DoesNotContain("{gameSummary}", rendered);
        Assert.DoesNotContain("{action}", rendered);
        Assert.DoesNotContain("{input}", rendered);
        Assert.DoesNotContain("{history}", rendered);
        Assert.DoesNotContain("{tools}", rendered);
        Assert.DoesNotContain("{tool_names}", rendered);
    }

    [Fact]
    public void NarrateWithGraphPrompt_RendersWithExactlyTheVariablesTheNarratorBindsForAScenario()
    {
        var prompt = new PromptTemplate(new EmbeddedInstructionCatalog().Get(InstructionKey.NarrateWithGraph));

        string rendered = prompt.Render(new Dictionary<string, string>
        {
            ["gameSummary"] = "<game summary>",
            ["action"] = "<action>",
            ["graph"] = "<graph>",
            ["input"] = "<input>",
            ["history"] = "<history>",
            ["tools"] = "<tools>",
            ["tool_names"] = "<tool names>",
        });

        Assert.DoesNotContain("{gameSummary}", rendered);
        Assert.DoesNotContain("{action}", rendered);
        Assert.DoesNotContain("{graph}", rendered);
        Assert.DoesNotContain("{input}", rendered);
        Assert.DoesNotContain("{history}", rendered);
        Assert.DoesNotContain("{tools}", rendered);
        Assert.DoesNotContain("{tool_names}", rendered);
    }

    [Fact]
    public void SearchScenarioPrompt_RendersWithExactlyTheVariablesTheToolBinds()
    {
        var prompt = new PromptTemplate(new EmbeddedInstructionCatalog().Get(InstructionKey.SearchScenario));

        string rendered = prompt.Render(new Dictionary<string, string>
        {
            ["gameSummary"] = "<game summary>",
            ["graph"] = "<graph>",
            ["context"] = "<context>",
            ["input"] = "<input>",
        });

        Assert.DoesNotContain("{gameSummary}", rendered);
        Assert.DoesNotContain("{graph}", rendered);
        Assert.DoesNotContain("{context}", rendered);
        Assert.DoesNotContain("{input}", rendered);
    }
}
