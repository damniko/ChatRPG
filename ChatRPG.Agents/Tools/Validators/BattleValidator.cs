using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Domain.Combat;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class BattleValidator(IToolDataValidator<ToolData.Character> characterValidator)
    : IToolDataValidator<ToolData.Battle>
{
    public bool IsValid(ToolData.Battle toolData, out IList<string> errors)
    {
        // TODO: Does the JSON deserializer support parsing enums directly?
        var participant1 = ValidateParticipant(nameof(toolData.Participant1), toolData.Participant1,
            toolData.Participant1HitChance, toolData.Participant1DamageSeverity);
            
        var participant2 = ValidateParticipant(nameof(toolData.Participant2), toolData.Participant2,
            toolData.Participant2HitChance, toolData.Participant2DamageSeverity);

        errors = [..participant1.Errors, ..participant2.Errors];
        
        return participant1.Errors.Count == 0 && participant2.Errors.Count == 0;
    }

    private ValidatedParticipant ValidateParticipant(string participant, ToolData.Character? character, string? hitChanceStr, string? damageSeverityStr)
    {
        IList<string> participantErrors = [];
        if (character == null)
            participantErrors.Add($"{participant} is required.");
        else if (!characterValidator.IsValid(character, out var characterErrors))
            participantErrors.Add($"{participant} is invalid: {string.Join(", ", characterErrors)}");

        if (!Enum.TryParse<HitChance>(hitChanceStr, true, out var hitChance))
            participantErrors.Add($"{participant}HitChance is required and must be one of: {string.Join(", ", Enum.GetNames<HitChance>())}");
        
        if (!Enum.TryParse<DamageSeverity>(damageSeverityStr, true, out var damageSeverity))
            participantErrors.Add($"{participant}DamageSeverity is required and must be one of: {string.Join(", ", Enum.GetNames<DamageSeverity>())}");
        
        return new ValidatedParticipant(participantErrors, hitChance, damageSeverity);
    }
    
    // TODO: The enums might be unnecessary here
    private sealed record ValidatedParticipant(IList<string> Errors, HitChance HitChance, DamageSeverity DamageSeverity);
}
