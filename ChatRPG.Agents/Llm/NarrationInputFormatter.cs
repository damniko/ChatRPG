using System.Text;
using ChatRPG.Application.Gameplay;

namespace ChatRPG.Agents.Llm;

internal static class NarrationInputFormatter
{
    public static string Format(NarrationRequest request)
    {
        return request switch
        {
            NarrationRequest.Opening opening => FormatOpening(opening),
            NarrationRequest.PlayerTurn turn => FormatPlayerTurn(turn),
            NarrationRequest.Epilogue epilogue => FormatEpilogue(epilogue),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }

    private static string FormatEpilogue(NarrationRequest.Epilogue epilogue)
    {
        return $"Player: {epilogue.Action.Text}\nGM: {epilogue.PrecedingNarration}";
    }

    private static string FormatOpening(NarrationRequest.Opening opening)
    {
        var sb = new StringBuilder()
            .Append("Opening:\n")
            .Append(opening.Scenario);

        AppendGraphSummary(sb, opening.GraphSummary);

        return sb.ToString();
    }

    private static string FormatPlayerTurn(NarrationRequest.PlayerTurn turn)
    {
        if (turn.Campaign.IsOpenWorld)
        {
            return turn.Action.Text;
        }

        var sb = new StringBuilder()
            .Append($"Player input ({turn.Action.Kind}):\n")
            .Append(turn.Action.Text);

        if (turn.Ruling is { } ruling)
        {
            sb.Append('\n').Append(ActionRulingFormatter.Format(ruling));
        }

        AppendGraphSummary(sb, turn.GraphSummary);

        return sb.ToString();
    }

    private static void AppendGraphSummary(StringBuilder sb, string? graphSummary)
    {
        if (graphSummary is not null)
        {
            sb.Append("\nGraph update summary:\n")
                .Append(graphSummary);
        }
    }
}
