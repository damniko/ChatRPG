using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Domain.Entities;
using LangChain.Chains.StackableChains.Agents.Tools;
using Environment = ChatRPG.Domain.Entities.Environment;

namespace ChatRPG.Agents.Tools.Implementations;

internal sealed class UpdateEnvironmentTool(
    Campaign campaign,
    IToolDescriptionCatalog descriptions, 
    IToolDataTextParser parser,
    IToolDataValidator<ToolData.Environment> validator) : AgentTool(ToolName, descriptions.Get(ToolDescriptionKey.UpdateEnvironment))
{
    private const string ToolName = "updateenvironmenttool";

    public override Task<string> ToolTask(string input, CancellationToken ct = default)
    {
        if (!parser.TryParse<ToolData.Environment>(input, out var toolData, out string? error))
        {
            return Task.FromResult($"Invalid input syntax: {error}");
        }
        var environmentData = toolData!;
        
        if (!validator.IsValid(environmentData, out var errors))
        {
            return Task.FromResult($"Invalid input: {string.Join(", ", errors)}");
        }

        var environment = campaign.Environments.FirstOrDefault(e => e.Name == environmentData.Name);
        if (environment is null)
        {
            environment = new Environment(campaign, environmentData.Name, environmentData.Description);
            campaign.Environments.Add(environment);
        }
        else
        {
            environment.Description = environmentData.Description!;
        }

        if (environmentData.IsPlayerHere)
        {
            campaign.Player.Environment = environment;
        }

        return Task.FromResult(
            $"Environment with name {environment.Name} was created or updated with the description: {environment.Description}");
    }
}
