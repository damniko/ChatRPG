using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
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
    IToolDataValidatorFactory validators,
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
        new(parser, graph, descriptions, validators.Create<ToolData.AddEdge>(), edgeConsistencyValidator);
    
    public AddNodeTool GetAddNodeTool(NarrativeGraph graph) =>
        new(parser, graph, descriptions, validators.Create<ToolData.AddNode>(), edgeConsistencyValidator);

    public AddEndNodeTool GetAddEndNodeTool(NarrativeGraph graph) 
        => new(graph, descriptions, parser, validators.Create<ToolData.AddEndNode>());

    public UpdateCharacterTool GetUpdateCharacterTool(IReadOnlyList<CharacterView> characters, ChangeCollector changes) =>
        new(characters, changes, parser, validators.Create<ToolData.Character>(), descriptions);

    public UpdateLocationTool GetUpdateLocationTool(IReadOnlyList<string> locations, ChangeCollector changes) =>
        new(locations, changes, descriptions, parser, validators.Create<ToolData.Location>());
    
    public SearchScenarioTool GetSearchScenarioTool(Campaign campaign, string gameSummary) =>
        new(campaign, gameSummary, documentStore, models, instructions, descriptions, parser, validators.Create<ToolData.SearchScenario>());
    
    public UpdateGraphTool GetUpdateGraphTool(Campaign campaign, string gameSummary, ActionRuling ruling) =>
        new(campaign, gameSummary, ruling, models, instructions, descriptions, parser, validators.Create<ToolData.UpdateGraph>());

    public BattleTool GetBattleTool(Campaign campaign) =>
        new(campaign, instructions, descriptions, parser, validators.Create<ToolData.Battle>(), characterFinder, randomSource, combatResolver);

    public HealCharacterTool GetHealCharacterTool(Campaign campaign) =>
        new(campaign, descriptions, instructions, parser, characterFinder, combatResolver);
    
    public WoundCharacterTool GetWoundCharacterTool(Campaign campaign) =>
        new(campaign, characterFinder, instructions, parser, combatResolver, descriptions);
}
