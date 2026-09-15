using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Application.Abstractions;
using LangChain.Providers;

namespace ChatRPG.Agents.Summarization;

/// <summary>Condenses each exchange into the running summary so it stays short enough to keep in every prompt.</summary>
internal sealed class LlmSummarizer(
    IChatModelFactory models,
    IInstructionCatalog instructions) : ISummarizer
{
    private const double Temperature = 0.4;

    public async Task<string> SummarizeAsync(SummaryRequest request, CancellationToken ct = default)
    {
        string prompt = new PromptTemplate(instructions.Get(InstructionKey.Summarize)).Render(
            new Dictionary<string, string>
            {
                ["gameSummary"] = request.CurrentSummary,
                ["new_lines"] = ExchangeTranscript.Format(request).TrimEnd()
            });

        var response = await models.CreateChat(Temperature, nameof(LlmSummarizer))
            .GenerateAsync(ChatRequest.ToChatRequest(prompt), settings: null, ct);

        return response.LastMessageContent;
    }
}
