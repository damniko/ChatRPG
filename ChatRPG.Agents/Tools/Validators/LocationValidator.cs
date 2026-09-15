using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

// TODO: Maybe we can use data annotations to simplify the validation framework?
internal sealed class LocationValidator : IToolDataValidator<ToolData.Location>
{
    public bool IsValid(ToolData.Location toolData, out IList<string> errors)
    {
        errors = [];
        if (string.IsNullOrWhiteSpace(toolData.Name))
            errors.Add("Name is required.");
        
        if (string.IsNullOrWhiteSpace(toolData.Description))
            errors.Add("Description is required.");

        return errors.Count == 0;
    }
}
