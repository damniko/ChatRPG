using LangChain.Chains.StackableChains.Agents.Tools;

namespace ChatRPG.Agents.ReAct;

internal sealed class ReActToolbox
{
    private readonly Dictionary<string, AgentTool> _tools;

    public ReActToolbox(IEnumerable<AgentTool> tools)
    {
        _tools = tools.ToDictionary(tool => tool.Name, StringComparer.OrdinalIgnoreCase);
    }

    public string Names => string.Join(",", _tools.Keys);

    public string Descriptions => string.Join("\n", _tools.Values.Select(tool => $"{tool.Name}, {tool.Description}"));

    public Task<string> ObserveAsync(string toolName, string input, CancellationToken ct = default)
    {
        return _tools.TryGetValue(toolName, out var tool)
            ? tool.ToolTask(input, ct)
            : Task.FromResult($"There is no tool named \"{toolName}\". Available tools: {Names}.");
    }
}
