using System.Text.Json;
using ChatRPG.Agents.Tools.Helpers;

namespace ChatRPG.Agents.Tools.Parsing;

internal class ToolDataTextParser : IToolDataTextParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public bool TryParse<T>(string input, out T? result, out string? error) where T : ToolData
    {
        result = null;
        error = null;
        try
        {
            result = JsonSerializer.Deserialize<T>(ToolInput.StripJsonDelimiter(input), JsonOptions);
        }
        catch (JsonException e)
        {
            error = e.Message;
        }
        return result != null;
    }
}
