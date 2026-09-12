using LangChain.Providers;
using Moq;

namespace ChatRPG.Agents.Tests;

/// <summary>
/// Stand-in chat models for the agent tests. A real provider would be a network call, so the tests
/// script what the model says and then read back the instructions it was sent.
/// </summary>
internal static class ChatModelStub
{
    /// <summary>A model that replies with the given outputs in order, repeating the last one.</summary>
    public static Mock<IChatModel> Answering(params string[] outputs)
    {
        var model = new Mock<IChatModel>();
        var remaining = new Queue<string>(outputs);

        model.Setup(m => m.GenerateAsync(It.IsAny<ChatRequest>(), It.IsAny<ChatSettings>(), It.IsAny<CancellationToken>()))
            .Returns(() => Respond(remaining.Count > 1 ? remaining.Dequeue() : remaining.Peek()));

        return model;
    }

    /// <summary>
    /// The same, but streaming: the provider yields a response per token carrying only a delta, then a
    /// last one carrying the whole message.
    /// </summary>
    public static Mock<IChatModel> Streaming(params string[] outputs)
    {
        var model = new Mock<IChatModel>();
        var remaining = new Queue<string>(outputs);

        model.Setup(m => m.GenerateAsync(It.IsAny<ChatRequest>(), It.IsAny<ChatSettings>(), It.IsAny<CancellationToken>()))
            .Returns(() => RespondInTokens(remaining.Count > 1 ? remaining.Dequeue() : remaining.Peek()));

        return model;
    }

    /// <summary>The instructions the model was asked to complete, in order.</summary>
    public static List<string> PromptsSentTo(Mock<IChatModel> model)
    {
        return [.. model.Invocations
            .Where(invocation => invocation.Method.Name == nameof(IChatModel.GenerateAsync))
            .Select(invocation => ((ChatRequest)invocation.Arguments[0]).Messages.Last().Content)];
    }

    public static async Task<List<string>> Collect(IAsyncEnumerable<string> chunks)
    {
        var collected = new List<string>();

        await foreach (string chunk in chunks)
        {
            collected.Add(chunk);
        }

        return collected;
    }

    private static async IAsyncEnumerable<ChatResponse> Respond(string output)
    {
        await Task.Yield();

        yield return new ChatResponse
        {
            Messages = [new Message(output, MessageRole.Ai)],
            UsedSettings = ChatSettings.Default,
        };
    }

    private static async IAsyncEnumerable<ChatResponse> RespondInTokens(string output)
    {
        await Task.Yield();

        // Three characters at a time, so "Final Answer:" lands across several deltas as it does in practice.
        for (int at = 0; at < output.Length; at += 3)
        {
            yield return new ChatResponse
            {
                Messages = [],
                UsedSettings = ChatSettings.Default,
                Delta = new ChatResponseDelta { Content = output.Substring(at, Math.Min(3, output.Length - at)) },
            };
        }

        yield return new ChatResponse
        {
            Messages = [new Message(output, MessageRole.Ai)],
            UsedSettings = ChatSettings.Default,
        };
    }
}
