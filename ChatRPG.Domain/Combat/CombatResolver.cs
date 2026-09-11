using ChatRPG.Domain.Entities;

namespace ChatRPG.Domain.Combat;

public sealed class CombatResolver(IRandomSource random)
{
    public BattleOutcome ResolveBattle(
        Character participant1,
        Character participant2,
        HitChance chance1,
        DamageSeverity severity1,
        HitChance chance2,
        DamageSeverity severity2)
    {
        bool p1Strikes = random.NextDouble() <= CombatRules.InitiativeProbability(participant1.Type, participant2.Type);

        var (first, second, firstChance, firstSeverity, secondChance, secondSeverity) = p1Strikes
            ? (participant1, participant2, chance1, severity1, chance2, severity2)
            : (participant2, participant1, chance2, severity2, chance1, severity1);

        var attacks = new List<AttackOutcome> { ResolveAttack(first, second, firstChance, firstSeverity) };

        // The second combatant only swings back if they are still standing.
        if (second.CurrentHealth > 0)
        {
            attacks.Add(ResolveAttack(second, first, secondChance, secondSeverity));
        }

        return new BattleOutcome(first, second, attacks);
    }
    
    public AttackOutcome ResolveAttack(
        Character attacker, Character defender, HitChance chance, DamageSeverity severity)
    {
        if (random.NextDouble() > CombatRules.ProbabilityOf(chance))
        {
            return new AttackOutcome(attacker, defender, Hit: false, Damage: 0, DefenderDied: false);
        }

        (int min, int max) = CombatRules.MagnitudeOf(severity);
        int damage = random.Next(min, max);
        bool died = defender.AdjustHealth(-damage);

        return new AttackOutcome(attacker, defender, Hit: true, damage, died);
    }
    
    public HealthChange Wound(Character character, DamageSeverity severity)                                                                                                                                                                                                                                       
      {                                                                                                                                                                                                                                                                                                             
          (int min, int max) = CombatRules.MagnitudeOf(severity);                                                                                                                                                                                                                                                       
          int damage = random.Next(min, max);                                                                                                                                                                                                                                                                       
          return new HealthChange(character, -damage, character.AdjustHealth(-damage));                                                                                                                                                                                                                             
      }                                                                                                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                                                                    
      public HealthChange Heal(Character character, DamageSeverity magnitude)                                                                                                                                                                                                                                       
      {                                                                                                                                                                                                                                                                                                             
          (int min, int max) = CombatRules.MagnitudeOf(magnitude);                                                                                                                                                                                                                                                      
          int healing = random.Next(min, max);                                                                                                                                                                                                                                                                      
          character.AdjustHealth(healing);                                                                                                                                                                                                                                                                          
          return new HealthChange(character, healing, Died: false);                                                                                                                                                                                                                                                 
      }
}
