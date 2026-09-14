using ChatRPG.Application.Usage;

// Not "…Tests.Usage": that would shadow the LangChain and Application types named Usage for every
// sibling test namespace.
namespace ChatRPG.Agents.Tests.UsageTracking;

public class LlmUsageAccumulatorTests
{
    private const int ContextLength = 100_000;

    private readonly LlmUsageAccumulator _accumulator = new();

    [Fact]
    public void RecordCall_SeveralCalls_SumsTurnAndSessionTotals()
    {
        _accumulator.RecordCall(Call("Examiner", prompt: 100, completion: 20, cost: 0.01m));
        _accumulator.RecordCall(Call("Narrator", prompt: 500, completion: 300, cost: 0.05m));

        var snapshot = _accumulator.Current;
        Assert.Equal(600, snapshot.TurnPromptTokens);
        Assert.Equal(320, snapshot.TurnCompletionTokens);
        Assert.Equal(920, snapshot.TurnTotalTokens);
        Assert.Equal(2, snapshot.TurnCallCount);
        Assert.Equal(0.06m, snapshot.TurnCostUsd);
        Assert.Equal(920, snapshot.SessionTotalTokens);
    }

    [Fact]
    public void BeginTurn_AfterATurn_ClearsTurnTotalsButKeepsSessionTotals()
    {
        _accumulator.RecordCall(Call("Narrator", prompt: 500, completion: 300, cost: 0.05m));

        _accumulator.BeginTurn();
        _accumulator.RecordCall(Call("Narrator", prompt: 200, completion: 100, cost: 0.02m));

        var snapshot = _accumulator.Current;
        Assert.Equal(300, snapshot.TurnTotalTokens);
        Assert.Equal(1, snapshot.TurnCallCount);
        Assert.Equal(0.02m, snapshot.TurnCostUsd);

        Assert.Equal(1100, snapshot.SessionTotalTokens);
        Assert.Equal(2, snapshot.SessionCallCount);
        Assert.Equal(0.07m, snapshot.SessionCostUsd);
    }

    [Fact]
    public void ContextUsedFraction_SeveralCalls_ReflectsTheLastCallRatherThanTheirSum()
    {
        // The window bounds one request, so these must not accumulate: after three calls the answer
        // is the last call's 2%, not the 8% their prompts would add up to.
        _accumulator.RecordCall(Call("Narrator", prompt: 1_000, completion: 10, cost: null));
        _accumulator.RecordCall(Call("Narrator", prompt: 5_000, completion: 10, cost: null));
        _accumulator.RecordCall(Call("Narrator", prompt: 2_000, completion: 10, cost: null));

        Assert.Equal(0.02, _accumulator.Current.ContextUsedFraction!.Value, precision: 4);
    }

    [Fact]
    public void RecordCall_MixedOperations_GroupsThemForTheBreakdown()
    {
        _accumulator.RecordCall(Call("Narrator", prompt: 100, completion: 50, cost: 0.01m));
        _accumulator.RecordCall(Call("Examiner", prompt: 20, completion: 5, cost: 0.002m));
        _accumulator.RecordCall(Call("Narrator", prompt: 200, completion: 70, cost: 0.03m));

        var byOperation = _accumulator.Current.ByOperation.ToDictionary(o => o.Operation);

        Assert.Equal(2, byOperation["Narrator"].Calls);
        Assert.Equal(300, byOperation["Narrator"].PromptTokens);
        Assert.Equal(120, byOperation["Narrator"].CompletionTokens);
        Assert.Equal(0.04m, byOperation["Narrator"].CostUsd);

        Assert.Equal(1, byOperation["Examiner"].Calls);
        Assert.Equal(25, byOperation["Examiner"].TotalTokens);
    }

    [Fact]
    public void TurnContainsEstimates_OneEstimatedCall_LatchesUntilTheNextTurn()
    {
        _accumulator.RecordCall(Call("Examiner", 100, 20, null, UsageAccuracy.Reported));
        Assert.False(_accumulator.Current.TurnContainsEstimates);

        _accumulator.RecordCall(Call("Narrator", 500, 300, null, UsageAccuracy.Estimated));
        Assert.True(_accumulator.Current.TurnContainsEstimates);

        // A later exact call must not erase the fact that the total is partly counted, not reported.
        _accumulator.RecordCall(Call("Archivist", 50, 10, null, UsageAccuracy.Reported));
        Assert.True(_accumulator.Current.TurnContainsEstimates);

        _accumulator.BeginTurn();
        Assert.False(_accumulator.Current.TurnContainsEstimates);
    }

    [Fact]
    public void CostUsd_NoCallKnowsItsPrice_StaysAbsentRatherThanReadingAsFree()
    {
        _accumulator.RecordCall(Call("Narrator", prompt: 100, completion: 20, cost: null));

        Assert.Null(_accumulator.Current.TurnCostUsd);
    }

    [Fact]
    public void ResetSession_AfterSeveralTurns_ClearsEverything()
    {
        _accumulator.RecordCall(Call("Narrator", prompt: 100, completion: 20, cost: 0.01m));

        _accumulator.ResetSession();

        var snapshot = _accumulator.Current;
        Assert.Equal(0, snapshot.SessionTotalTokens);
        Assert.Equal(0, snapshot.TurnTotalTokens);
        Assert.Null(snapshot.LastCall);
        Assert.Empty(snapshot.ByOperation);
    }

    [Fact]
    public void RecordCall_FromManyThreads_LosesNothing()
    {
        // Tool calls within a turn can overlap, so the fold has to hold up under concurrency.
        Parallel.For(0, 1_000, _ =>
            _accumulator.RecordCall(Call("Narrator", prompt: 10, completion: 5, cost: 0.001m)));

        var snapshot = _accumulator.Current;
        Assert.Equal(1_000, snapshot.SessionCallCount);
        Assert.Equal(15_000, snapshot.SessionTotalTokens);
        Assert.Equal(1_000, snapshot.ByOperation.Single().Calls);
    }

    private static LlmCallUsage Call(
        string operation,
        int prompt,
        int completion,
        decimal? cost,
        UsageAccuracy accuracy = UsageAccuracy.Reported) =>
        new(operation, "gpt-4o", prompt, completion, ContextLength, TimeSpan.FromSeconds(1), cost, accuracy);
}
