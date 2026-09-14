using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Usage;

namespace ChatRPG.Agents.Tests;

/// <summary>Keeps every reported call so a test can assert on what was recorded, and in what order.</summary>
internal sealed class RecordingUsageSink : ILlmUsageSink
{
    private readonly Lock _gate = new();
    private readonly List<LlmCallUsage> _calls = [];

    public IReadOnlyList<LlmCallUsage> Calls
    {
        get
        {
            lock (_gate)
            {
                return [.. _calls];
            }
        }
    }

    public LlmCallUsage Single => Calls.Single();

    public void RecordCall(LlmCallUsage usage)
    {
        lock (_gate)
        {
            _calls.Add(usage);
        }
    }
}
