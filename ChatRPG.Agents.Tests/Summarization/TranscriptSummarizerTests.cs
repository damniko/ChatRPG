using ChatRPG.Agents.Summarization;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tests.Summarization;

public class TranscriptSummarizerTests
{
    [Fact]
    public async Task SummarizeAsync_NoRuling_AppendsThePlayerAndGmLines()
    {
        var summarizer = new TranscriptSummarizer();

        string summary = await summarizer.SummarizeAsync(
            new SummaryRequest(string.Empty, "I open the door.", "The door swings open.", null));

        Assert.Equal("Player: I open the door.\nGM: The door swings open.\n\n", summary);
    }

    [Fact]
    public async Task SummarizeAsync_WithRuling_PutsTheRulingBetweenThem()
    {
        var summarizer = new TranscriptSummarizer();
        var ruling = new ActionRuling(ActionPermission.Conditional, "The door is barred from the other side.");

        string summary = await summarizer.SummarizeAsync(
            new SummaryRequest(string.Empty, "I open the door.", "It does not budge.", ruling));

        Assert.Equal(
            "Player: I open the door.\n"
            + "Ruling: CONDITIONAL\nReasoning: The door is barred from the other side.\n"
            + "GM: It does not budge.\n\n",
            summary);
    }

    [Fact]
    public async Task SummarizeAsync_ExistingSummary_KeepsItAndAppendsAfterIt()
    {
        var summarizer = new TranscriptSummarizer();

        string summary = await summarizer.SummarizeAsync(
            new SummaryRequest("Player: I enter.\nGM: A hall.\n\n", "I look up.", "A chandelier.", null));

        Assert.Equal(
            "Player: I enter.\nGM: A hall.\n\nPlayer: I look up.\nGM: A chandelier.\n\n",
            summary);
    }
}
