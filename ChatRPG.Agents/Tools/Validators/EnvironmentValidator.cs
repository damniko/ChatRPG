using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

// TODO: Maybe we can use data annotations to simplify the validation framework?
internal sealed class EnvironmentValidator : IToolDataValidator<ToolData.Environment>
{
    public bool IsValid(ToolData.Environment toolData, out IList<string> errors)
    {
        errors = [];
        if (string.IsNullOrWhiteSpace(toolData.Name))
            errors.Add("Name is required.");
        
        if (string.IsNullOrWhiteSpace(toolData.Description))
            errors.Add("Description is required.");

        return errors.Count == 0;
    }
}
