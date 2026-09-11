using System.Text;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Helpers;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Domain.Combat;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class WoundCharacterTool(
    Campaign campaign,
    CharacterFinder characterFinder,
    IInstructionCatalog instructions,
    IToolDataTextParser parser,
    CombatResolver resolver,
    IToolDescriptionCatalog descriptions) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.WoundCharacter))
{
    private const string ToolName = "woundcharactertool";

    public override async Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.Wound>(input, out var toolData, out string? error))
            return $"Invalid input syntax: {error}";

        var wound = toolData!;

        var character = await characterFinder.FindAsync(campaign, wound.Input!, instructions.Get(InstructionKey.WoundCharacter), ct);
        if (character == null)
            return "Could not determine the character to wound. The character does not exist in the game.";

        _ = Enum.TryParse<DamageSeverity>(wound.Severity, true, out var severity);
        var outcome = resolver.Wound(character, severity);

        var result = new StringBuilder();
        
        if (outcome.Died)
        {
            result.Append($"The character {character.Name} is wounded for {outcome.Amount} damage and has died.");
            result.Append(character.IsPlayer
                ? "With the player dead, the adventure is over. No more actions can be taken."
                : "They can no longer perform actions in the narrative.");
        }
        else
        {
            result.Append(
                $"The character {character.Name} is wounded for {outcome.Amount} damage. They have {character.CurrentHealth} of {character.MaxHealth} health points remaining.");
        }
        return result.ToString();
    }
}
