using LangChain.Providers;

namespace ChatRPG.Agents.Llm;

internal interface IChatModelFactory
{
    /// <param name="operation">
    /// Who is asking, e.g. <c>nameof(ReActNarratorAgent)</c>. Selects debug output and labels the
    /// call's token usage, so leaving it null makes that usage unattributable.
    /// </param>
    IChatModel CreateChat(double temperature, string? operation = null, bool streaming = false);
    IEmbeddingModel CreateEmbedding();
    ITextToImageModel CreateTextToImage();
}
