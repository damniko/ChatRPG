using ChatRPG.Application.Abstractions;

namespace ChatRPG.Agents.Llm;

internal static class ArchiveInputFormatter
{
    public static string Format(ArchiveRequest request)
    {
        return request switch
        {
            ArchiveRequest.Turn turn => FormatTurn(turn),
            ArchiveRequest.Opening opening => FormatOpening(opening),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }

    private static string FormatTurn(ArchiveRequest.Turn turn)
    {
        return $"The player says: {turn.PlayerInput}\nThe DM says: {turn.Narration}";
    }

    private static string FormatOpening(ArchiveRequest.Opening opening)
    {
        return $"The DM says: {opening.Narration}";
    }
}
