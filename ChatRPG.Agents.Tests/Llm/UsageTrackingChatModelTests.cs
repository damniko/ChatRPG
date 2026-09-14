using ChatRPG.Agents.Llm;
using ChatRPG.Application.Usage;
using LangChain.Providers;
using Moq;

namespace ChatRPG.Agents.Tests.Llm;

public class UsageTrackingChatModelTests
{
    private const int ContextLength = 128_000;

    private readonly RecordingUsageSink _sink = new();

    [Fact]
    public async Task GenerateAsync_ProviderReportsUsage_RecordsItAsReported()
    {
        var model = Wrap(Sequence(Final("Hello", new Usage(120, 34, 1, TimeSpan.Zero, 0.25))));

        await Drain(model);

        var recorded = _sink.Single;
        Assert.Equal(120, recorded.PromptTokens);
        Assert.Equal(34, recorded.CompletionTokens);
        Assert.Equal(154, recorded.TotalTokens);
        Assert.Equal(0.25m, recorded.CostUsd);
        Assert.Equal(UsageAccuracy.Reported, recorded.Accuracy);
        Assert.Equal("Narrator", recorded.Operation);
    }

    [Fact]
    public async Task GenerateAsync_ProviderReportsNoUsage_CountsTokensItself()
    {
        // What a streamed call looks like: deltas, a final message, and no usage anywhere.
        var model = Wrap(Sequence(Delta("Hel"), Delta("lo"), Final("Hello", usage: null)));

        await Drain(model);

        var recorded = _sink.Single;
        Assert.Equal(UsageAccuracy.Estimated, recorded.Accuracy);
        Assert.True(recorded.PromptTokens > 0);
        Assert.True(recorded.CompletionTokens > 0);
    }

    [Fact]
    public async Task GenerateAsync_StreamCarriesEmptyDeltas_CountsTheTextAroundThem()
    {
        // A real stream brackets its text with content-free deltas, so those must be treated as
        // deltas rather than as the response carrying the finished message.
        var model = Wrap(Sequence(
            Delta(string.Empty), Delta("Hello"), Delta(string.Empty), Final("Hello", usage: null)));

        var seen = await Drain(model);

        Assert.Equal(4, seen.Count);
        Assert.Equal(UsageAccuracy.Estimated, _sink.Single.Accuracy);
        Assert.True(_sink.Single.CompletionTokens > 0);
    }

    [Fact]
    public async Task GenerateAsync_CallerStopsEarly_StillRecordsUsage()
    {
        // ReActAgent abandons the enumerator as soon as it has a final answer, so a call that is
        // never drained still has to be paid for.
        var model = Wrap(Sequence(
            Delta("one"), Delta("two"), Final("onetwo", new Usage(10, 5, 1, TimeSpan.Zero, null))));

        await foreach (var _ in model.GenerateAsync(Request()))
        {
            break;
        }

        Assert.Single(_sink.Calls);
    }

    [Fact]
    public async Task GenerateAsync_ProviderThrowsPartway_RecordsUsageAndRethrows()
    {
        var model = Wrap(Failing(new InvalidOperationException("provider fell over")));

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => Drain(model));

        Assert.Equal("provider fell over", thrown.Message);
        Assert.Single(_sink.Calls);
    }

    [Fact]
    public async Task GenerateAsync_Always_PassesEveryResponseThroughUntouched()
    {
        var responses = new[] { Delta("a"), Delta("b"), Final("ab", null) };
        var model = Wrap(Sequence(responses));

        var seen = await Drain(model);

        Assert.Equal(responses, seen);
    }

    [Fact]
    public async Task GenerateAsync_ModelKnowsItsWindow_RecordsItSoTheFractionCanBeComputed()
    {
        var model = Wrap(Sequence(Final("Hello", new Usage(32_000, 10, 1, TimeSpan.Zero, null))));

        await Drain(model);

        Assert.Equal(ContextLength, _sink.Single.ContextWindowTokens);
        Assert.Equal(0.25, _sink.Single.ContextUsedFraction!.Value, precision: 4);
    }

    [Fact]
    public async Task GenerateAsync_CalledRepeatedly_RecordsOncePerCall()
    {
        // Every ReAct step is a separate billable call, so a multi-step run must not report one total.
        var model = Wrap(() => Sequence(Final("Hello", new Usage(10, 5, 1, TimeSpan.Zero, null))));

        await Drain(model);
        await Drain(model);
        await Drain(model);

        Assert.Equal(3, _sink.Calls.Count);
    }

    private UsageTrackingChatModel Wrap(IAsyncEnumerable<ChatResponse> responses) =>
        Wrap(() => responses);

    private UsageTrackingChatModel Wrap(Func<IAsyncEnumerable<ChatResponse>> responses)
    {
        var inner = new Mock<IChatModel>();
        inner.SetupGet(m => m.Id).Returns("gpt-4o");
        inner.SetupGet(m => m.ContextLength).Returns(ContextLength);
        inner.Setup(m => m.GenerateAsync(
                It.IsAny<ChatRequest>(), It.IsAny<ChatSettings>(), It.IsAny<CancellationToken>()))
            .Returns(responses);

        return new UsageTrackingChatModel(inner.Object, "Narrator", _sink, TimeProvider.System);
    }

    private static async Task<List<ChatResponse>> Drain(IChatModel model)
    {
        var seen = new List<ChatResponse>();

        await foreach (var response in model.GenerateAsync(Request()))
        {
            seen.Add(response);
        }

        return seen;
    }

    private static ChatRequest Request() => ChatRequest.ToChatRequest("Tell me what happens next.");

    private static ChatResponse Delta(string content) => new()
    {
        Messages = [],
        UsedSettings = ChatSettings.Default,
        Delta = new ChatResponseDelta { Content = content },
    };

    private static ChatResponse Final(string content, Usage? usage) => new()
    {
        Messages = [new Message(content, MessageRole.Ai)],
        UsedSettings = ChatSettings.Default,
        Usage = usage ?? Usage.Empty,
    };

    private static async IAsyncEnumerable<ChatResponse> Sequence(params ChatResponse[] responses)
    {
        foreach (var response in responses)
        {
            await Task.Yield();
            yield return response;
        }
    }

    private static async IAsyncEnumerable<ChatResponse> Failing(Exception failure)
    {
        await Task.Yield();
        yield return Delta("partial");

        throw failure;
    }
}
