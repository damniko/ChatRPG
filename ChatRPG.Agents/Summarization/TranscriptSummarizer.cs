using ChatRPG.Application.Abstractions;

namespace ChatRPG.Agents.Summarization;

/// <summary>Keeps the whole story verbatim by appending each exchange to the running summary.</summary>
internal sealed class TranscriptSummarizer : ISummarizer
{
    public Task<string> SummarizeAsync(SummaryRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(request.CurrentSummary + ExchangeTranscript.Format(request));
    }
}
