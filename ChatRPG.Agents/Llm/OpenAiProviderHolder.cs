using ChatRPG.Agents.Configuration;
using LangChain.Providers.OpenAI;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.Llm;

internal sealed class OpenAiProviderHolder(IOptions<LanguageModelOptions> llmOptions)
{
    public OpenAiProvider Provider { get; } = new(llmOptions.Value.ApiKey);
}
