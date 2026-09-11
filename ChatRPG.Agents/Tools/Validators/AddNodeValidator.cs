using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal sealed class AddNodeValidator(IToolDataValidator<ToolData.AddEdge> edgeValidator)
    : IToolDataValidator<ToolData.AddNode>
{
    public bool IsValid(ToolData.AddNode toolData, out IList<string> errors)
    {
        errors = [];
        
        if (string.IsNullOrWhiteSpace(toolData.Name))
            errors.Add("Name is required.");
        
        if (string.IsNullOrWhiteSpace(toolData.StoryContent))
            errors.Add("StoryContent is required.");

        if (toolData.Edges.Count == 0)
        {
            errors.Add("Edges must contain at least one edge.");
        }
        else
        {
            foreach (var edge in toolData.Edges)
            {
                if (!edgeValidator.IsValid(edge, out IList<string> edgeErrors))
                {
                    errors.Add(string.Join(", ", edgeErrors.Select(e => $"Edge: {e}")));
                }
            }
        }
        
        return errors.Count == 0;
    }
}
