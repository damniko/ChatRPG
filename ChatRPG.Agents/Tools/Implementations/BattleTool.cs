using System.Text;
using System.Text.Json;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Helpers;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Domain.Combat;
using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class BattleTool(
    Campaign campaign,
    IInstructionCatalog instructions,
    IToolDescriptionCatalog descriptions,
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.Battle> validator,
    CharacterFinder characterFinder,
    IRandomSource randomSource,
    CombatResolver combatResolver) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.Battle))
{
    private const string ToolName = "battletool";

    public override async Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.Battle>(input, out var toolData, out string? error))
            return $"Invalid input syntax: {error}";

        var battle = toolData!;

        if (!validator.IsValid(battle, out var errors))
            return $"Invalid input: {string.Join(", ", errors)}";

        var character1 = await FindOrCreateCharacterAsync(battle.Participant1!, ct);
        var character2 = await FindOrCreateCharacterAsync(battle.Participant2, ct);
        
        // TODO: Use combatResolver.ResolveBattle instead
        var attackTurn = BuildAttackTurn(character1, character2, battle);

        var result = new StringBuilder();
        
        var firstOutcome = combatResolver.ResolveAttack(attackTurn.FirstHitter, attackTurn.SecondHitter, attackTurn.FirstHitChance, attackTurn.FirstHitSeverity);
        AppendAttackOutcome(firstOutcome, ref result);

        if (!firstOutcome.DefenderDied)
        {
            var secondOutcome = combatResolver.ResolveAttack(attackTurn.SecondHitter, attackTurn.FirstHitter, attackTurn.SecondHitChance, attackTurn.SecondHitSeverity);
            AppendAttackOutcome(secondOutcome, ref result);
        }

        result.AppendLine($"The battle has been resolved between {character1.Name} and {character2.Name}, and this pair cannot be used for the battle tool again.");
        return result.ToString();
    }

    private async Task<Character> FindOrCreateCharacterAsync(ToolData.Character participant, CancellationToken ct = default)
    {
        string json = JsonSerializer.Serialize(new { participant.Name, participant.Description });
        var character = await characterFinder.FindAsync(campaign, json,  instructions.Get(InstructionKey.Battle), ct);
        
        // Create dummy characters if they do not exist and pray that the archive chain will update them
        character ??= new Character(campaign, campaign.Player.Location, CharacterType.Humanoid, participant.Name!, participant.Description!, false);

        return character;
    }

    private Character DetermineFirstHitter(Character participant1, Character participant2)
    {
        double roll = randomSource.NextDouble();
        double initiativeProbability = CombatRules.InitiativeProbability(participant1.Type, participant2.Type);
        
        // TODO: Double check this
        return roll >= initiativeProbability ? participant1 : participant2;
    }

    private AttackTurn BuildAttackTurn(Character character1, Character character2, ToolData.Battle battle)
    {
        var firstHitter = DetermineFirstHitter(character1, character2);
        Character secondHitter;
        HitChance firstHitChance;
        HitChance secondHitChance;
        DamageSeverity firstHitSeverity;
        DamageSeverity secondHitSeverity;

        if (firstHitter == character1)
        {
            secondHitter = character2;
            firstHitChance = Enum.Parse<HitChance>(battle.Participant1HitChance!);
            secondHitChance = Enum.Parse<HitChance>(battle.Participant2HitChance!);
            firstHitSeverity = Enum.Parse<DamageSeverity>(battle.Participant1DamageSeverity!);
            secondHitSeverity = Enum.Parse<DamageSeverity>(battle.Participant2DamageSeverity!);
        }
        else
        {
            secondHitter = character1;
            firstHitChance = Enum.Parse<HitChance>(battle.Participant2HitChance!);
            secondHitChance = Enum.Parse<HitChance>(battle.Participant1HitChance!);
            firstHitSeverity = Enum.Parse<DamageSeverity>(battle.Participant2DamageSeverity!);
            secondHitSeverity = Enum.Parse<DamageSeverity>(battle.Participant1DamageSeverity!);
        }

        return new AttackTurn(firstHitter, secondHitter, firstHitChance, secondHitChance, firstHitSeverity,
            secondHitSeverity);
    }

    private void AppendAttackOutcome(AttackOutcome outcome, ref StringBuilder result)
    {
        result.AppendLine(
            $"{outcome.Attacker.Name} described as '{outcome.Attacker.Description}' fights {outcome.Defender.Name} described as '{outcome.Defender.Description}'");

        if (outcome.Hit)
        {
            result.AppendLine($"{outcome.Attacker.Name} deals {outcome.Damage} damage to {outcome.Defender.Name}.");
            if (outcome.DefenderDied)
            {
                if (outcome.Defender.IsPlayer)
                {
                    result.AppendLine(
                        $"The player {outcome.Defender.Name} has died. Their adventure is over, and no more actions can be taken.");
                
                }
                else
                {
                    result.AppendLine(
                        $"The character {outcome.Defender.Name} hs died, and can no longer take action in the narrative.");
                }
            }
            else
            {
                result.AppendLine(
                    $"The character {outcome.Defender.Name} now has {outcome.Defender.CurrentHealth} of {outcome.Defender.MaxHealth} health points remaining.");
            }
        }
        else
        {
            result.Append($"{outcome.Attacker.Name} misses their attack on {outcome.Defender.Name}.");
        }

    }

    private sealed record AttackTurn(
        Character FirstHitter,
        Character SecondHitter,
        HitChance FirstHitChance,
        HitChance SecondHitChance,
        DamageSeverity FirstHitSeverity,
        DamageSeverity SecondHitSeverity);
}
