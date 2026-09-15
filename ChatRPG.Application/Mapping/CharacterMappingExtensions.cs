using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Mapping;

public static class CharacterMappingExtensions
{
    extension(Character character)
    {
        public CharacterView ToView()
        {
            return new CharacterView(character.Id, character.Name, character.Description, character.CurrentHealth,
                character.IsPlayer, character.Type);
        }
    }
}