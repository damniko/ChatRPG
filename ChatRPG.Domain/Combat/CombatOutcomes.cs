using ChatRPG.Domain.Entities;

namespace ChatRPG.Domain.Combat;

public sealed record AttackOutcome(
    Character Attacker,
    Character Defender,
    bool Hit,
    int Damage,
    bool DefenderDied);

public sealed record BattleOutcome(
    Character Initiator,
    Character Responder,
    IReadOnlyList<AttackOutcome> Attacks);

public sealed record HealthChange(Character Character, int Amount, bool Died)
{
    public bool IsHealing => Amount > 0;
}
