using System.Collections.Frozen;
using System.Reflection;

namespace ChatRPG.Agents;

/// <summary>
/// Reads the markdown embedded alongside this assembly - the instructions and the tool descriptions.
/// Every entry is loaded once, up front, so a missing or empty file fails fast instead of surfacing
/// as an empty prompt halfway through a turn.
/// </summary>
internal static class EmbeddedMarkdown
{
    /// <summary>
    /// Loads one file per member of <typeparamref name="TKey" />, which makes the enum the single
    /// list of what must exist on disk.
    /// </summary>
    public static FrozenDictionary<TKey, string> LoadAll<TKey>(Func<TKey, string> resourceNameFor)
        where TKey : struct, Enum
    {
        var assembly = typeof(EmbeddedMarkdown).Assembly;
        var loaded = new Dictionary<TKey, string>();
        var missing = new List<string>();

        foreach (var key in Enum.GetValues<TKey>())
        {
            string resourceName = resourceNameFor(key);
            string? text = Read(assembly, resourceName);

            if (string.IsNullOrWhiteSpace(text))
            {
                missing.Add(resourceName);
                continue;
            }

            loaded[key] = text.Trim();
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing or empty embedded resources: {string.Join(", ", missing)}. " +
                "Ensure the files exist and are included as EmbeddedResource.");
        }

        return loaded.ToFrozenDictionary();
    }

    private static string? Read(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
