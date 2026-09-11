using LangChain.Providers;

namespace ChatRPG.Agents.Llm;

internal interface IChatModelFactory
{
    // TODO: Do something else with the agent parameter
    IChatModel CreateChat(double temperature, string? agent = null, bool streaming = false);
    IEmbeddingModel CreateEmbedding();
}
