using System.Text;

namespace ChatRPG.Agents.ReAct;

/// <summary>
/// Holds back the model's reasoning while it streams, and lets through only what follows
/// <see cref="ReActOutputParser.FinalAnswerMarker" /> - the part the player is meant to read.
/// </summary>
/// <remarks>
/// One filter belongs to one model call. The marker can be split across any number of deltas,
/// so it is matched against everything seen so far rather than against a single delta.
/// </remarks>
internal sealed class FinalAnswerFilter
{
    private StringBuilder? _withheld = new();

    /// <summary>
    /// Feeds one delta to the filter and returns the text to stream, or null while the model
    /// is still reasoning its way towards a final answer.
    /// </summary>
    public string? Consume(string delta)
    {
        // Past the marker, everything the model writes is part of the answer.
        if (_withheld is null)
        {
            return delta;
        }

        string seenSoFar = _withheld.Append(delta).ToString();

        int marker = seenSoFar.IndexOf(ReActOutputParser.FinalAnswerMarker, StringComparison.Ordinal);
        if (marker < 0)
        {
            return null;
        }

        _withheld = null;

        // Only the space the model writes after the marker is dropped; the rest streams verbatim.
        return seenSoFar[(marker + ReActOutputParser.FinalAnswerMarker.Length)..].TrimStart();
    }
}
