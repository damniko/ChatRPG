using System.Runtime.CompilerServices;
using System.Text;
using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Usage;
using CSharpToJsonSchema;
using LangChain.Providers;

namespace ChatRPG.Agents.Llm;

/// <summary>
/// Wraps a chat model and reports what each call cost. Everything else is passed straight through.
/// </summary>
internal sealed class UsageTrackingChatModel(
    IChatModel inner,
    string operation,
    ILlmUsageSink sink,
    TimeProvider clock)
    : IChatModel, ISupportsCountTokens, IPaidLargeLanguageModel
{
    /// <summary>Rough fallback when the model cannot count tokens: about four characters each.</summary>
    private const int CharactersPerToken = 4;

    public IAsyncEnumerable<ChatResponse> GenerateAsync(
        ChatRequest request,
        ChatSettings? settings = null,
        CancellationToken cancellationToken = default) =>
        TrackAsync(request, settings, cancellationToken);

    private async IAsyncEnumerable<ChatResponse> TrackAsync(
        ChatRequest request,
        ChatSettings? settings,
        [EnumeratorCancellation] CancellationToken ct)
    {
        long started = clock.GetTimestamp();

        var reported = Usage.Empty;
        var streamed = new StringBuilder();
        string? finalText = null;

        try
        {
            await foreach (var response in inner.GenerateAsync(request, settings, ct))
            {
                if (response.Usage is { } usage)
                {
                    reported += usage;
                }

                if (response.Delta is { } delta)
                {
                    streamed.Append(delta.Content);
                }
                else
                {
                    finalText = response.LastMessageContent;
                }

                yield return response;
            }
        }
        finally
        {
            Report(request, reported, finalText ?? streamed.ToString(), clock.GetElapsedTime(started));
        }
    }

    private void Report(ChatRequest request, Usage reported, string output, TimeSpan elapsed)
    {
        bool exact = reported.InputTokens > 0 || reported.OutputTokens > 0;

        int prompt = exact ? reported.InputTokens : CountPrompt(request);
        int completion = exact ? reported.OutputTokens : CountText(output);

        double? price = exact ? reported.PriceInUsd : TryCalculatePriceInUsd(prompt, completion);

        sink.RecordCall(new LlmCallUsage(
            operation,
            inner.Id,
            prompt,
            completion,
            inner.ContextLength > 0 ? inner.ContextLength : null,
            elapsed,
            price is { } usd ? (decimal)usd : null,
            exact ? UsageAccuracy.Reported : UsageAccuracy.Estimated));
    }

    private int CountPrompt(ChatRequest request) =>
        inner is ISupportsCountTokens counter
            ? counter.CountTokens(request)
            : Approximate(request.Messages.Sum(message => message.Content.Length));

    private int CountText(string text) =>
        inner is ISupportsCountTokens counter ? counter.CountTokens(text) : Approximate(text.Length);

    private static int Approximate(int characters) => characters / CharactersPerToken;

    // Forwarded so that library code type-testing the model still finds these through the wrapper.

    public int CountTokens(string text) =>
        inner is ISupportsCountTokens counter ? counter.CountTokens(text) : Approximate(text.Length);

    public int CountTokens(IReadOnlyCollection<Message> messages) =>
        inner is ISupportsCountTokens counter
            ? counter.CountTokens(messages)
            : Approximate(messages.Sum(message => message.Content.Length));

    public int CountTokens(ChatRequest request) => CountPrompt(request);

    public double? TryCalculatePriceInUsd(int inputTokens, int outputTokens) =>
        inner is IPaidLargeLanguageModel paid
            ? paid.TryCalculatePriceInUsd(inputTokens, outputTokens)
            : null;

    public string Id => inner.Id;

    public Usage Usage => inner.Usage;

    public void AddUsage(Usage usage) => inner.AddUsage(usage);

    public ChatSettings? Settings
    {
        get => inner.Settings;
        set => inner.Settings = value;
    }

    public int ContextLength => inner.ContextLength;

    public bool CallToolsAutomatically
    {
        get => inner.CallToolsAutomatically;
        set => inner.CallToolsAutomatically = value;
    }

    public bool ReplyToToolCallsAutomatically
    {
        get => inner.ReplyToToolCallsAutomatically;
        set => inner.ReplyToToolCallsAutomatically = value;
    }

    public void AddGlobalTools(
        ICollection<Tool> tools,
        IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>> calls) =>
        inner.AddGlobalTools(tools, calls);

    public void ClearGlobalTools() => inner.ClearGlobalTools();

    public event EventHandler<ChatRequest>? RequestSent
    {
        add => inner.RequestSent += value;
        remove => inner.RequestSent -= value;
    }

    public event EventHandler<ChatResponseDelta>? DeltaReceived
    {
        add => inner.DeltaReceived += value;
        remove => inner.DeltaReceived -= value;
    }

    public event EventHandler<ChatResponse>? ResponseReceived
    {
        add => inner.ResponseReceived += value;
        remove => inner.ResponseReceived -= value;
    }
}
