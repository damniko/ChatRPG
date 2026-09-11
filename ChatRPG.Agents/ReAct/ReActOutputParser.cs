namespace ChatRPG.Agents.ReAct;

internal abstract record ReActStep
{
    public sealed record FinalAnswer(string Text) : ReActStep;

    public sealed record UseTool(string Name, string Input) : ReActStep;

    public sealed record Unparsable(string Output) : ReActStep;
}

internal static class ReActOutputParser
{
    internal const string FinalAnswerMarker = "Final Answer:";
    private const string ActionMarker = "Action:";
    private const string ActionInputMarker = "Action Input:";

    public static ReActStep Parse(string output)
    {
        int finalAnswer = output.IndexOf(FinalAnswerMarker, StringComparison.Ordinal);
        if (finalAnswer >= 0)
        {
            return new ReActStep.FinalAnswer(output[(finalAnswer + FinalAnswerMarker.Length)..].Trim());
        }

        int actionInput = output.IndexOf(ActionInputMarker, StringComparison.Ordinal);
        if (actionInput < 0)
        {
            return new ReActStep.Unparsable(output);
        }

        // Search for "Action:" only before the input marker, since "Action Input:" contains it.
        int action = output.LastIndexOf(ActionMarker, actionInput, StringComparison.Ordinal);
        if (action < 0)
        {
            return new ReActStep.Unparsable(output);
        }

        string name = output[(action + ActionMarker.Length)..actionInput].Trim();
        string input = output[(actionInput + ActionInputMarker.Length)..].Trim();

        return name.Length == 0
            ? new ReActStep.Unparsable(output)
            : new ReActStep.UseTool(name, input);
    }
}
