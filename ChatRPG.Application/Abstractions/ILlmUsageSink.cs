using ChatRPG.Application.Usage;

namespace ChatRPG.Application.Abstractions;

/// <summary>Where the agents report what each model call cost.</summary>
/// <remarks>
/// The agents push to this rather than returning usage through the gameplay ports, so none of the
/// narrative interfaces have to carry telemetry. Implementations must be thread-safe, since tool
/// calls within a turn can overlap, and must not throw: a failure to record usage must never fail
/// a turn.
/// </remarks>
public interface ILlmUsageSink
{
    void RecordCall(LlmCallUsage usage);
}
