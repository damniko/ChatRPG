namespace ChatRPG.Agents.ReAct.Exceptions;

/// <summary>
/// Thrown when an agent spends its whole step budget without ever writing a final answer - usually
/// a prompt that lets the model keep reasoning, or a tool whose observations never satisfy it.
/// </summary>
public sealed class NoFinalAnswerReachedException(string message, int steps, string lastOutput) : Exception(message)
{
    /// <summary>The step budget that was exhausted.</summary>
    public int Steps { get; } = steps;

    /// <summary>The last thing the model wrote, which is where diagnosis usually starts.</summary>
    public string LastOutput { get; } = lastOutput;
}
