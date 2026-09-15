using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Summarization;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using LangChain.Providers;
using Moq;

namespace ChatRPG.Agents.Tests.Summarization;

public class LlmSummarizerTests
{
    private const string SummarizePrompt = "Current summary:\n{gameSummary}\n\nNew lines:\n{new_lines}";

    [Fact]
    public async Task SummarizeAsync_ReturnsWhatTheModelWrote()
    {
        var model = ChatModelStub.Answering("The player opened the door and stepped through.");

        string summary = await SummarizerFor(model).SummarizeAsync(
            new SummaryRequest("The player stands before a door.", "I open the door.", "It swings open.", null));

        Assert.Equal("The player opened the door and stepped through.", summary);
    }

    [Fact]
    public async Task SummarizeAsync_SendsTheCurrentSummaryAndTheExchange()
    {
        var model = ChatModelStub.Answering("<summary>");
        var ruling = new ActionRuling(ActionPermission.Conditional, "The door is barred.");

        await SummarizerFor(model).SummarizeAsync(
            new SummaryRequest("The player stands before a door.", "I open the door.", "It does not budge.", ruling));

        string prompt = Assert.Single(ChatModelStub.PromptsSentTo(model));
        Assert.Contains("The player stands before a door.", prompt);
        Assert.Contains("Player: I open the door.", prompt);
        Assert.Contains("Ruling: CONDITIONAL\nReasoning: The door is barred.", prompt);
        Assert.Contains("GM: It does not budge.", prompt);
        Assert.DoesNotContain("{gameSummary}", prompt);
        Assert.DoesNotContain("{new_lines}", prompt);
    }

    private static LlmSummarizer SummarizerFor(Mock<IChatModel> model)
    {
        var models = new Mock<IChatModelFactory>();
        models.Setup(m => m.CreateChat(It.IsAny<double>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(model.Object);

        var instructions = new Mock<IInstructionCatalog>();
        instructions.Setup(i => i.Get(InstructionKey.Summarize)).Returns(SummarizePrompt);

        return new LlmSummarizer(models.Object, instructions.Object);
    }
}
