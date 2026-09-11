using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class AddEdgeValidator : IToolDataValidator<ToolData.AddEdge>
{
    public bool IsValid(ToolData.AddEdge toolData, out IList<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(toolData.SourceNodeName))
            errors.Add("SourceNodeName is required.");

        if (string.IsNullOrWhiteSpace(toolData.TargetNodeName))
            errors.Add("TargetNodeName is required.");

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
