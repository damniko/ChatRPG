using System.Collections.Frozen;

namespace ChatRPG.Agents.Tools.Catalogs;

/// <summary>
/// Serves the tool descriptions embedded alongside this assembly as
/// <c>Tools/Descriptions/{ToolDescriptionKey}.md</c>.
/// </summary>
internal sealed class EmbeddedToolDescriptionCatalog : IToolDescriptionCatalog
{
    private readonly FrozenDictionary<ToolDescriptionKey, string> _descriptions =
        EmbeddedMarkdown.LoadAll<ToolDescriptionKey>(
            key => $"ChatRPG.Agents.Tools.Descriptions.{key}.md");

    public string Get(ToolDescriptionKey key) => _descriptions[key];
}
