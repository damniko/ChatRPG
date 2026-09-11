using ChatRPG.Agents.Tools.Implementations;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Tools;

internal interface IToolFactory
{
    AddEdgeTool GetAddEdgeTool(NarrativeGraph graph);
    AddNodeTool GetAddNodeTool(NarrativeGraph graph);
    AddEndNodeTool GetAddEndNodeTool(NarrativeGraph graph);
    UpdateCharacterTool GetUpdateCharacterTool(Campaign campaign);
    UpdateEnvironmentTool GetUpdateEnvironmentTool(Campaign campaign);
    SearchScenarioTool GetSearchScenarioTool(Campaign campaign);
    UpdateGraphTool GetUpdateGraphTool(Campaign campaign, AdherenceVerdict verdict);
    BattleTool GetBattleTool(Campaign campaign);
    WoundCharacterTool GetWoundCharacterTool(Campaign campaign);
    HealCharacterTool GetHealCharacterTool(Campaign campaign);
}
