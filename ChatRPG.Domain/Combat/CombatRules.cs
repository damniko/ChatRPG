using ChatRPG.Domain.Enums;

namespace ChatRPG.Domain.Combat;

public static class CombatRules
{
    private static readonly Dictionary<HitChance, double> HitProbabilities = new()
    {
        [HitChance.High] = 0.9,
        [HitChance.Medium] = 0.5,
        [HitChance.Low] = 0.3,
        [HitChance.Impossible] = 0.01
    };

    private static readonly Dictionary<DamageSeverity, (int Min, int Max)> Magnitudes = new()
    {
        [DamageSeverity.Harmless] = (0, 1),
        [DamageSeverity.Low] = (5, 10),
        [DamageSeverity.Medium] = (10, 20),
        [DamageSeverity.High] = (15, 25),
        [DamageSeverity.Extraordinary] = (25, 80)
    };
    
    /// <summary>How dangerous each character type is, for initiative purposes.</summary>
    private static readonly Dictionary<CharacterType, int> ThreatRanks = new()
    {
        [CharacterType.SmallMonster] = 0,
        [CharacterType.Humanoid] = 1,
        [CharacterType.MediumMonster] = 2,
        [CharacterType.LargeMonster] = 3,
        [CharacterType.BossMonster] = 4
    };

    public static double ProbabilityOf(HitChance chance) => HitProbabilities[chance];

    public static (int Min, int Max) MagnitudeOf(DamageSeverity severity) => Magnitudes[severity];
    
    /// <summary>Probability that <paramref name="attacker"/> strikes before <paramref name="defender"/>.</summary>
    public static double InitiativeProbability(CharacterType attacker, CharacterType defender) =>
        (ThreatRanks[attacker] - ThreatRanks[defender]) switch
        {
            0 => 0.5,
            1 => 0.4,
            2 => 0.3,
            >= 3 => 0.2,
            -1 => 0.6,
            -2 => 0.7,
            <= -3 => 0.8
        };
}
