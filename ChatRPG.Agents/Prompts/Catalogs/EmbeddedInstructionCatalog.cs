using System.Collections.Frozen;

namespace ChatRPG.Agents.Prompts.Catalogs;

/// <summary>
/// Serves the instructions embedded alongside this assembly as <c>Prompts/{InstructionKey}.md</c>.
/// </summary>
internal sealed class EmbeddedInstructionCatalog : IInstructionCatalog
{
    private readonly FrozenDictionary<InstructionKey, string> _prompts =
        EmbeddedMarkdown.LoadAll<InstructionKey>(key => $"ChatRPG.Agents.Prompts.Instructions.{key}.md");

    public string Get(InstructionKey key) => _prompts[key];
}
