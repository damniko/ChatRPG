using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class CharacterValidator : IToolDataValidator<ToolData.Character>
{
    public bool IsValid(ToolData.Character toolData, out IList<string> errors)
    {
        errors = [];
        if (string.IsNullOrWhiteSpace(toolData.Name))
            errors.Add("Name is required.");
        
        if (string.IsNullOrWhiteSpace(toolData.Description))
            errors.Add("Description is required.");

        if (string.IsNullOrWhiteSpace(toolData.Type))
            errors.Add("Type is required.");
        
        if (string.IsNullOrWhiteSpace(toolData.State))
            errors.Add("State is required.");

        return errors.Count == 0;
    }
}
