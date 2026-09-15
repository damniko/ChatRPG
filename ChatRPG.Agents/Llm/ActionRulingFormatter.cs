using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Llm;

/// <summary>
/// The single rendering of an <see cref="ActionRuling" /> shown to the models.
/// Must match the response format in <c>Examine.md</c>.
/// </summary>
internal static class ActionRulingFormatter
{
    public static string Format(ActionRuling ruling)
    {
        return $"Ruling: {Token(ruling.Permission)}\nReasoning: {ruling.Reasoning}";
    }

    private static string Token(ActionPermission permission) => permission switch
    {
        ActionPermission.Allowed => "ALLOWED",
        ActionPermission.Conditional => "CONDITIONAL",
        ActionPermission.Disallowed => "DISALLOWED",
        _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
    };
}
