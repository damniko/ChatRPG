namespace ChatRPG.Application.Usage;

/// <summary>The read side of usage tracking, for whoever renders it.</summary>
public interface ILlmUsageReadout
{
    LlmUsageSnapshot Current { get; }
}

/// <summary>
/// The control side, for whoever drives turns. Kept separate from <see cref="ILlmUsageReadout" /> so
/// that something which only displays usage cannot reset it.
/// </summary>
/// <remarks>
/// Turn and session boundaries are set by calling these rather than inferred from a service
/// lifetime. That keeps the accumulator usable wherever it is hosted: a Blazor circuit, a request
/// scope, or a console process all call the same two methods, and only the DI registration differs.
/// </remarks>
public interface ILlmUsageTracker : ILlmUsageReadout
{
    /// <summary>Starts a new turn: clears the turn totals and keeps the session totals.</summary>
    void BeginTurn();

    /// <summary>Clears everything.</summary>
    void ResetSession();
}
