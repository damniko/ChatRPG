using ChatRPG.Agents.Configuration;
using LangChain.Providers;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.Extensions.Options;
using tryAGI.OpenAI;

namespace ChatRPG.Agents.Llm;

internal sealed class OpenAiChatModelFactory(
    IOptions<AgentOptions> agentOptions,
    IOptions<LanguageModelOptions> llmOptions) : IChatModelFactory
{
    private readonly OpenAiProvider _provider = new(llmOptions.Value.ApiKey);

    public IChatModel CreateChat(double temperature, string? agent = null, bool streaming = false)
    {
        var model = new Gpt4OmniModel(_provider)
        {
            Settings = new OpenAiChatSettings
            {
                UseStreaming = streaming,
                Temperature = temperature
            }
        };
        return agent != null && agentOptions.Value.DebugAgents.Contains(agent)
            ? model.UseConsoleForDebug()
            : model;
    }

    public IEmbeddingModel CreateEmbedding() => new TextEmbeddingV3SmallModel(_provider);

    public ITextToImageModel CreateTextToImage() => new OpenAiTextToImageModel(_provider, CreateImageRequestModel.DallE3);
}
