using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Agents.Tools;

internal sealed class ChangeCollector
{
    private readonly List<CharacterChange> _characterChanges = [];
    public IReadOnlyList<CharacterChange> CharacterChanges => _characterChanges;
    
    private readonly List<NewCharacter> _newCharacters = [];
    public IReadOnlyList<NewCharacter> NewCharacters => _newCharacters;

    public void Wound(int characterId, int amount) =>
        _characterChanges.Add(new CharacterChange(characterId, -amount, null));
    
    public void Describe(int characterId, string newDescription) =>
        _characterChanges.Add(new CharacterChange(characterId, 0, newDescription));

    public void NewCharacter(string name, string description, CharacterType type) =>
        _newCharacters.Add(new NewCharacter(name, description, type));

    private readonly List<LocationChange> _locationChanges = [];
    public IReadOnlyList<LocationChange> LocationChanges => _locationChanges;

    public void Location(string name, string description, bool isPlayerHere) =>
        _locationChanges.Add(new LocationChange(name, description, isPlayerHere));
}