using ChatRPG.Domain.Entities.Abstractions;
using ChatRPG.Domain.Enums;

namespace ChatRPG.Domain.Entities;

public class Character : IEntity
{
    public Character(Campaign campaign, Environment environment, CharacterType type, string name, string description,
        bool isPlayer)
    {
        Campaign = campaign;
        Environment = environment;
        Type = type;
        Name = name;
        Description = description;
        IsPlayer = isPlayer;
        MaxHealth = type switch
        {
            CharacterType.Humanoid => 40,
            CharacterType.SmallMonster => 15,
            CharacterType.MediumMonster => 35,
            CharacterType.LargeMonster => 55,
            CharacterType.BossMonster => 90,
            _ => 50
        };
        if (isPlayer)
            MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }

    public int Id { get; init; }
    public Campaign Campaign { get; private set; }
    public Environment Environment { get; set; }
    public CharacterType Type { get; private set; }
    public bool IsPlayer { get; private set; }
    public string Name { get; private set; }
    public string Description { get; set; }
    public byte[]? Portrait { get; init; }
    public int MaxHealth { get; init; }
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Adjust the current health of this character.
    /// </summary>
    /// <param name="value">The value to adjust the current health with.</param>
    public bool AdjustHealth(int value)
    {
        CurrentHealth = Math.Min(MaxHealth, Math.Max(0, CurrentHealth + value));
        return CurrentHealth <= 0;
    }
}
