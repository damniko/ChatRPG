using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface ISummarizer
{
    /// <summary>Folds one exchange into the running campaign summary and returns the new summary.</summary>
    Task<string> SummarizeAsync(SummaryRequest request, CancellationToken ct = default);
}

public sealed record SummaryRequest(
    string CurrentSummary,
    string PlayerInput,
    string Narration,
    ActionRuling? Ruling);
