using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class AddEndNodeValidator : IToolDataValidator<ToolData.AddEndNode>
{
    public bool IsValid(ToolData.AddEndNode toolData, out IList<string> errors)
    {
        errors = [];
        
        if (string.IsNullOrWhiteSpace(toolData.SourceNodeName))
            errors.Add("SourceNodeName is required.");
        
        if (toolData.Conditions.Count == 0)
        {
            errors.Add("Conditions must contain at least one condition.");
        }
        else
        {
            foreach (string _ in toolData.Conditions.Where(string.IsNullOrWhiteSpace))
            {
                errors.Add("Condition is required.");
            }
        }
        
        return errors.Count == 0;
    }
}
