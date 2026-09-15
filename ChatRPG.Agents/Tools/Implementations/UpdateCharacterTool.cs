using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Enums;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class UpdateCharacterTool(
    IReadOnlyList<CharacterView> characters,
    ChangeCollector changes,
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.Character> validator,
    IToolDescriptionCatalog descriptions) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.UpdateCharacter))
{
    private const string ToolName = "updatecharactertool";
    
    public override Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.Character>(input, out var toolData, out string? error))
        {
            return Task.FromResult($"Invalid input syntax: {error}");
        }
        var characterData = toolData!;
        
        if (!validator.IsValid(characterData, out var errors))
        {
            return Task.FromResult($"Invalid input: {string.Join(", ", errors)}");
        }
        
        _ = Enum.TryParse<CharacterType>(characterData.Type, out var characterType);

        var character = characters.FirstOrDefault(c => c.Name == characterData.Name && c.Type == characterType);
        if (character == null)
        {
            changes.NewCharacter(characterData.Name!, characterData.Description!, characterType);
            return Task.FromResult($"A new character named {characterData.Name} has been created with description: {characterData.Description}");
        }
        
        changes.Describe(character.Id, characterData.Description!);
        return Task.FromResult($"{character.Name} has been updated with the description: {characterData.Description}");
    }
    
    // TODO: This should be a domain rule with a domain enum
    private static double ScaleHealthBasedOnState(string state)
    {
        return state switch
        {
            "Dead" => 0,
            "Unconscious" => 0.1,
            "HeavilyWounded" => 0.35,
            "LightlyWounded" => 0.75,
            "Healthy" => 1,
            _ => 1
        };
    }
}
