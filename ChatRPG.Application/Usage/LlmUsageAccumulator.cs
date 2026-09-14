using ChatRPG.Application.Abstractions;

namespace ChatRPG.Application.Usage;

/// <summary>
/// Folds the calls the agents report into a running snapshot. One object with two faces: the agents
/// see only <see cref="ILlmUsageSink" />, whoever displays or drives it sees
/// <see cref="ILlmUsageTracker" />.
/// </summary>
public sealed class LlmUsageAccumulator : ILlmUsageSink, ILlmUsageTracker
{
    private readonly Lock _gate = new();

    private readonly Dictionary<string, OperationUsage> _byOperation = new(StringComparer.Ordinal);

    private LlmUsageSnapshot _current = LlmUsageSnapshot.Empty;

    public LlmUsageSnapshot Current
    {
        get
        {
            lock (_gate)
            {
                return _current;
            }
        }
    }

    public void RecordCall(LlmCallUsage usage)
    {
        ArgumentNullException.ThrowIfNull(usage);

        lock (_gate)
        {
            _byOperation[usage.Operation] = Fold(
                _byOperation.GetValueOrDefault(usage.Operation), usage);

            var previous = _current;

            _current = previous with
            {
                TurnPromptTokens = previous.TurnPromptTokens + usage.PromptTokens,
                TurnCompletionTokens = previous.TurnCompletionTokens + usage.CompletionTokens,
                TurnCallCount = previous.TurnCallCount + 1,
                TurnCostUsd = Add(previous.TurnCostUsd, usage.CostUsd),

                SessionPromptTokens = previous.SessionPromptTokens + usage.PromptTokens,
                SessionCompletionTokens = previous.SessionCompletionTokens + usage.CompletionTokens,
                SessionCallCount = previous.SessionCallCount + 1,
                SessionCostUsd = Add(previous.SessionCostUsd, usage.CostUsd),

                LastCall = usage,
                ByOperation = [.. _byOperation.Values],
                TurnContainsEstimates =
                    previous.TurnContainsEstimates || usage.Accuracy is UsageAccuracy.Estimated,
            };
        }
    }

    public void BeginTurn()
    {
        lock (_gate)
        {
            _current = _current with
            {
                TurnPromptTokens = 0,
                TurnCompletionTokens = 0,
                TurnCallCount = 0,
                TurnCostUsd = null,
                TurnContainsEstimates = false,
            };
        }
    }

    public void ResetSession()
    {
        lock (_gate)
        {
            _byOperation.Clear();
            _current = LlmUsageSnapshot.Empty;
        }
    }

    private static OperationUsage Fold(OperationUsage? previous, LlmCallUsage usage) =>
        previous is null
            ? new OperationUsage(usage.Operation, 1, usage.PromptTokens, usage.CompletionTokens, usage.CostUsd)
            : previous with
            {
                Calls = previous.Calls + 1,
                PromptTokens = previous.PromptTokens + usage.PromptTokens,
                CompletionTokens = previous.CompletionTokens + usage.CompletionTokens,
                CostUsd = Add(previous.CostUsd, usage.CostUsd),
            };

    /// <summary>Sums two costs, treating "no price known" as absent rather than as zero.</summary>
    private static decimal? Add(decimal? left, decimal? right) =>
        left is null && right is null ? null : (left ?? 0m) + (right ?? 0m);
}
