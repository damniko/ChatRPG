using ChatRPG.Agents.Configuration;
using ChatRPG.Application.Abstractions;
using LangChain.Providers;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;
using Microsoft.Extensions.Options;
using tryAGI.OpenAI;

namespace ChatRPG.Agents.Llm;

internal sealed class OpenAiChatModelFactory(
    OpenAiProviderHolder providers,
    IOptions<AgentOptions> agentOptions,
    ILlmUsageSink usageSink,
    TimeProvider clock) : IChatModelFactory
{
    /// <summary>Stands in for the caller when one did not identify itself.</summary>
    private const string UnattributedOperation = "Unattributed";

    private OpenAiProvider Provider => providers.Provider;

    public IChatModel CreateChat(double temperature, string? operation = null, bool streaming = false)
    {
        var model = new Gpt4OmniModel(Provider)
        {
            Settings = new OpenAiChatSettings
            {
                UseStreaming = streaming,
                Temperature = temperature
            }
        };

        IChatModel chat = operation != null && agentOptions.Value.DebugAgents.Contains(operation)
            ? model.UseConsoleForDebug()
            : model;

        // Outside the debug decorator, so debug output is unaffected by tracking.
        return new UsageTrackingChatModel(
            chat, operation ?? UnattributedOperation, usageSink, clock);
    }

    public IEmbeddingModel CreateEmbedding() => new TextEmbeddingV3SmallModel(Provider);

    public ITextToImageModel CreateTextToImage() =>
        new OpenAiTextToImageModel(Provider, CreateImageRequestModel.DallE3);
}
