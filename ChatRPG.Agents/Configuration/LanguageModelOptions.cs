namespace ChatRPG.Agents.Configuration;

public sealed class LanguageModelOptions
{
    public const string Section = "LanguageModel";

    public string ApiKey { get; init; } = "";
    public string ChatModel { get; init; } = "";
    public string EmbeddingModel { get; init; } = "";
    public string ImageModel { get; init; } = "";
}
