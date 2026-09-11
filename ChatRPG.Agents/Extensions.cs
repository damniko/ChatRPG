using ChatRPG.Application.Abstractions;

namespace ChatRPG.Agents;

public static class Extensions
{
    public static string ToPromptString(this AdherenceVerdict verdict)
    {
        return $"Allowed: {verdict.IsAllowed}\nReasoning: {verdict.Reasoning}\n";
    }
}
