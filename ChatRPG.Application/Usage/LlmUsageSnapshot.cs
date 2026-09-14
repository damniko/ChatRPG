namespace ChatRPG.Application.Usage;

/// <summary>Everything a status readout needs, as one immutable value.</summary>
public sealed record LlmUsageSnapshot
{
    public static readonly LlmUsageSnapshot Empty = new();

    public int TurnPromptTokens { get; init; }
    public int TurnCompletionTokens { get; init; }
    public int TurnCallCount { get; init; }
    public decimal? TurnCostUsd { get; init; }

    public int SessionPromptTokens { get; init; }
    public int SessionCompletionTokens { get; init; }
    public int SessionCallCount { get; init; }
    public decimal? SessionCostUsd { get; init; }

    /// <summary>The call that produced this snapshot. Drives the context gauge.</summary>
    public LlmCallUsage? LastCall { get; init; }

    /// <summary>Per-agent breakdown for the session, so a readout can show a total and expand it.</summary>
    public IReadOnlyList<OperationUsage> ByOperation { get; init; } = [];

    /// <summary>True once any call in the turn was counted locally rather than reported.</summary>
    public bool TurnContainsEstimates { get; init; }

    public int TurnTotalTokens => TurnPromptTokens + TurnCompletionTokens;

    public int SessionTotalTokens => SessionPromptTokens + SessionCompletionTokens;

    /// <summary>
    /// How full the context window was on the most recent call. Derived from <see cref="LastCall" />
    /// alone, never accumulated, because a sum of per-request fractions is meaningless.
    /// </summary>
    public double? ContextUsedFraction => LastCall?.ContextUsedFraction;

    public int? ContextWindowTokens => LastCall?.ContextWindowTokens;
}

/// <summary>What one agent or tool has cost so far this session.</summary>
public sealed record OperationUsage(
    string Operation,
    int Calls,
    int PromptTokens,
    int CompletionTokens,
    decimal? CostUsd)
{
    public int TotalTokens => PromptTokens + CompletionTokens;
}
