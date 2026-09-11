using System.Text.RegularExpressions;

namespace ChatRPG.Agents.Prompts;

internal sealed partial class PromptTemplate(string template)
{
    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex Placeholder();

    /// <summary>
    /// Replaces all placeholders in the template with the provided variables.
    /// </summary>
    /// <exception cref="InvalidOperationException">A variable was bound that the template does not specify.</exception>
    public string Render(IReadOnlyDictionary<string, string> variables)
    {
        var unused = new HashSet<string>(variables.Keys, StringComparer.Ordinal);

        string rendered = Placeholder().Replace(template, match =>
        {
            string name = match.Groups[1].Value;
            if (!variables.TryGetValue(name, out string? value))
            {
                return match.Value;
            }

            unused.Remove(name);
            return value;
        });

        if (unused.Count > 0)
        {
            throw new InvalidOperationException(
                $"The prompt has no placeholder for: {string.Join(", ", unused.Order())}. " +
                "Either the prompt or the code binding it is out of date.");
        }

        return rendered;
    }
}
