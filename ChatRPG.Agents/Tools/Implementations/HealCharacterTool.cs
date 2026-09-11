using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Helpers;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Domain.Combat;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class HealCharacterTool(
    Campaign campaign,
    IToolDescriptionCatalog descriptions,
    IInstructionCatalog instructions,
    IToolDataTextParser parser,
    CharacterFinder characterFinder,
    CombatResolver resolver) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.HealCharacter))
{
    private const string ToolName = "healcharactertool";

    public override async Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.Heal>(input, out var toolData, out string? error))
            return $"Invalid input syntax: {error}";

        var healData = toolData!;

        var character = await characterFinder.FindAsync(campaign, healData.Input!, instructions.Get(InstructionKey.HealCharacter), ct);
        if (character is null)
            return "Failed to determine the character to heal. No action has been performed.";
        
        _ = Enum.TryParse<DamageSeverity>(healData.Magnitude, true, out var severity);
        var healthChange = resolver.Heal(character, severity);

        return
            $"The character {healthChange.Character.Name} is healed for {healthChange.Amount} health points. They now have {healthChange.Character.CurrentHealth} of {healthChange.Character.MaxHealth} health points.";
    }
}
