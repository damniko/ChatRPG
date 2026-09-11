namespace ChatRPG.Agents.Tools.Parsing;

internal interface IToolDataTextParser
{
    bool TryParse<T>(string input, out T? result, out string? error) where T : ToolData;
}
