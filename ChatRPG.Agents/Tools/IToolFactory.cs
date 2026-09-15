using ChatRPG.Agents.Tools.Implementations;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tools;

internal interface IToolFactory
{
    AddEdgeTool GetAddEdgeTool(NarrativeGraph graph);
    AddNodeTool GetAddNodeTool(NarrativeGraph graph);
    AddEndNodeTool GetAddEndNodeTool(NarrativeGraph graph);
    UpdateCharacterTool GetUpdateCharacterTool(IReadOnlyList<CharacterView> characters, ChangeCollector changes);
    UpdateLocationTool GetUpdateLocationTool(IReadOnlyList<string> locations, ChangeCollector changes);
    SearchScenarioTool GetSearchScenarioTool(Campaign campaign, string gameSummary);
    UpdateGraphTool GetUpdateGraphTool(Campaign campaign, string gameSummary, ActionRuling ruling);
    BattleTool GetBattleTool(Campaign campaign);
    WoundCharacterTool GetWoundCharacterTool(Campaign campaign);
    HealCharacterTool GetHealCharacterTool(Campaign campaign);
}
