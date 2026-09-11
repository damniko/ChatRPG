using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class UpdateGraphValidator : IToolDataValidator<ToolData.UpdateGraph>
{
    public bool IsValid(ToolData.UpdateGraph toolData, out IList<string> errors)
    {
        errors = [];
        if (string.IsNullOrEmpty(toolData.SourceNodeName))
            errors.Add($"{nameof(toolData.SourceNodeName)} is required.");
        if (string.IsNullOrEmpty(toolData.TargetNodeName))
            errors.Add($"{nameof(toolData.TargetNodeName)} is required.");
        
        return errors.Count == 0;
    }
}
