using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Helpers;
using ChatRPG.Agents.Tools.Implementations;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Combat;
using ChatRPG.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.Tools;

internal class ToolFactory(
    IToolDataTextParser parser,
    IToolDataValidatorFactory validatorFactory,
    IToolDescriptionCatalog descriptions, 
    EdgeConsistencyValidator edgeConsistencyValidator,
    IOptions<AgentOptions> options,
    IScenarioDocumentStore documentStore,
    IChatModelFactory models,
    IInstructionCatalog instructions,
    CharacterFinder characterFinder,
    CombatResolver combatResolver,
    IRandomSource randomSource) : IToolFactory
{
    public AddEdgeTool GetAddEdgeTool(NarrativeGraph graph) => 
        new(parser, graph, descriptions, validatorFactory.Create<ToolData.AddEdge>(), edgeConsistencyValidator);
    
    public AddNodeTool GetAddNodeTool(NarrativeGraph graph) =>
        new(parser, graph, descriptions, validatorFactory.Create<ToolData.AddNode>(), edgeConsistencyValidator);

    public AddEndNodeTool GetAddEndNodeTool(NarrativeGraph graph) 
        => new(graph, descriptions);

    public UpdateCharacterTool GetUpdateCharacterTool(Campaign campaign) =>
        new(parser, campaign, validatorFactory.Create<ToolData.Character>(), descriptions);

    public UpdateEnvironmentTool GetUpdateEnvironmentTool(Campaign campaign) =>
        new(campaign, descriptions, parser, validatorFactory.Create<ToolData.Environment>());
    
    public SearchScenarioTool GetSearchScenarioTool(Campaign campaign) => 
        new(campaign, GameSummaryFormatter.Format(campaign, options.Value.IncludePreviousMessages), documentStore, models, instructions, descriptions, parser, validatorFactory.Create<ToolData.SearchScenario>());
    
    public UpdateGraphTool GetUpdateGraphTool(Campaign campaign, AdherenceVerdict verdict) =>
        new(campaign, GameSummaryFormatter.Format(campaign, options.Value.IncludePreviousMessages), verdict, models, instructions, descriptions, parser, validatorFactory.Create<ToolData.UpdateGraph>());

    public BattleTool GetBattleTool(Campaign campaign) =>
        new(campaign, instructions, descriptions, parser, validatorFactory.Create<ToolData.Battle>(), characterFinder, randomSource, combatResolver);

    public HealCharacterTool GetHealCharacterTool(Campaign campaign) =>
        new(campaign, descriptions, instructions, parser, characterFinder, combatResolver);
    
    public WoundCharacterTool GetWoundCharacterTool(Campaign campaign) =>
        new(campaign, characterFinder, instructions, parser, combatResolver, descriptions);
}
