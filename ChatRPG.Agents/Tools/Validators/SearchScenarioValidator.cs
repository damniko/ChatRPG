using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class SearchScenarioValidator : IToolDataValidator<ToolData.SearchScenario>
{
    public bool IsValid(ToolData.SearchScenario toolData, out IList<string> errors)
    {
        errors = [];
        if (string.IsNullOrWhiteSpace(toolData.Query))
            errors.Add("Query is required.");
        
        return errors.Count == 0;
    }
}
