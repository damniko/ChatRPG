namespace ChatRPG.Agents.Configuration;

public sealed class AgentOptions
{
    public const string Section = "Agents";

    public bool StreamChatCompletions { get; init; } = true;
    public bool IncludePreviousMessages { get; init; } = true;
    public int PreviousMessagesCount { get; init; } = 4;
    public bool SummarizeArchivistMessages { get; init; } = true;
    public bool VisualizeNarrativeGraph { get; init; }
    public HashSet<string> DebugAgents { get; init; } = [];
    public int ScribeBatchSize { get; init; } = 5;
}
