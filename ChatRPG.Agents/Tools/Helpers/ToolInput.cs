namespace ChatRPG.Agents.Tools.Helpers;

internal static class ToolInput
{
    /// <summary>
    /// Strips JSON block delimiters (<c>```json ```</c>) from the given text.
    /// </summary>
    /// <param name="input">The input to strip.</param>
    /// <returns>The stripped input.</returns>
    public static string StripJsonDelimiter(string input)
    {
        string trimmed = input.Trim();

        return trimmed.StartsWith("```json", StringComparison.Ordinal) && trimmed.EndsWith("```", StringComparison.Ordinal)
            ? trimmed.Replace("```json", "").Replace("```", "")
            : input;
    }
}
