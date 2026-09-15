using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class UpdateLocationTool(
    IReadOnlyList<string> locations,
    ChangeCollector changes,
    IToolDescriptionCatalog descriptions,
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.Location> validator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.UpdateLocation))
{
    private const string ToolName = "updatelocationtool";

    public override Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.Location>(input, out var toolData, out string? error))
        {
            return Task.FromResult($"Invalid input syntax: {error}");
        }
        var locationData = toolData!;
        
        if (!validator.IsValid(locationData, out var errors))
        {
            return Task.FromResult($"Invalid input: {string.Join(", ", errors)}");
        }

        changes.Location(locationData.Name!, locationData.Description!, locationData.IsPlayerHere);

        string verb = locations.Contains(locationData.Name) ? "updated" : "created";
        return Task.FromResult(
            $"Location with name {locationData.Name} was {verb} with the description: {locationData.Description}");
    }
}
