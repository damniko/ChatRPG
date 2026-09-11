using System.Text;

namespace ChatRPG.Agents.ReAct;

/// <summary>
/// The history of what the agent has tried so far.
/// </summary>
internal sealed class ReActScratchpad
{
    private readonly StringBuilder _steps = new();

    public void Record(string modelOutput, string observation)
    {
        if (_steps.Length > 0)
        {
            _steps.Append('\n');
        }

        _steps.Append(modelOutput.Trim())
            .Append("\nObservation: ").Append(observation)
            .Append("\nThought:");
    }

    public override string ToString() => _steps.ToString();
}
