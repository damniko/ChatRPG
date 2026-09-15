namespace ChatRPG.Agents.Tools.Parsing;

internal abstract record ToolData
{
    internal sealed record Character(string? Name, string? Description, string? Type, string? State) : ToolData;
    internal sealed record Location(string Name, string Description, bool IsPlayerHere) : ToolData;
    internal sealed record AddNode(string Name, string StoryContent, List<AddEdge> Edges) : ToolData;

    internal sealed record AddEdge(List<string> Conditions, string SourceNodeName, string TargetNodeName) : ToolData;
    
    internal sealed record AddEndNode(string SourceNodeName, List<string> Conditions) :  ToolData;

    internal sealed record SearchScenario(string? Query, string? NodeName) : ToolData;
    internal sealed record UpdateGraph(string? SourceNodeName, string? TargetNodeName) : ToolData;

    internal sealed record EdgeConditions(Dictionary<string, bool>? Conditions) : ToolData;

    internal sealed record Battle(
        Character? Participant1,
        string? Participant1HitChance,
        string? Participant1DamageSeverity,
        Character Participant2,
        string? Participant2HitChance,
        string? Participant2DamageSeverity) : ToolData;

    internal sealed record Heal(string? Input, string? Magnitude) : ToolData;
    internal sealed record Wound(string? Input, string? Severity) : ToolData;
}