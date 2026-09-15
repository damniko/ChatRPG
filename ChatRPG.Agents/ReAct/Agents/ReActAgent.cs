using System.Runtime.CompilerServices;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.ReAct.Exceptions;
using LangChain.Chains.StackableChains.Agents.Tools;
using LangChain.Providers;

namespace ChatRPG.Agents.ReAct.Agents;

/// <summary>
/// Runs one agent through the ReAct loop: render the prompt, ask the model what to do, run the tool
/// it names, and feed the result back as an observation until it writes a final answer.
/// </summary>
/// <example>
/// <code>
/// var agent = new ReActAgent(model, instructions.Get(InstructionKey.Examine))
/// {
///     Variables = { ["graph"] = graph, ["gameSummary"] = campaign.GameSummary },
///     Tools = { searchScenarioTool },
/// };
///
/// string verdict = await agent.RunAsync(playerInput, ct);
/// </code>
/// </example>
internal sealed class ReActAgent(IChatModel model, string prompt)
{
    private static readonly ChatSettings ReActFormat = new()
    {
        StopSequences = ["Observation", "[END]"],
    };

    private const string FormatReminder =
        "That did not follow the required format. Answer with \"Action:\" and \"Action Input:\" to " +
        "use a tool, or with \"Final Answer:\" if you are done.";

    /// <summary>Bindings for the placeholders this agent's prompt declares.</summary>
    /// <remarks>
    /// Binding a variable the prompt does not ask for is an error (see <see cref="PromptTemplate" />).
    /// </remarks>
    public Dictionary<string, string> Variables { get; } = new(StringComparer.Ordinal);

    public List<AgentTool> Tools { get; } = [];

    /// <summary>How many tool calls the agent may make before it has to answer.</summary>
    public int MaxSteps { get; init; } = 20;

    /// <exception cref="NoFinalAnswerReachedException">The agent never reached a final answer.</exception>
    public async Task<string> RunAsync(string input, CancellationToken ct = default)
    {
        var run = StartRun(input);

        string output = string.Empty;

        for (int step = 0; step < MaxSteps; step++)
        {
            var response = await model.GenerateAsync(run.NextRequest(), ReActFormat, ct);

            output = response.LastMessageContent;

            var next = ReActOutputParser.Parse(output);
            if (next is ReActStep.FinalAnswer answer)
            {
                return answer.Text;
            }

            await run.RecordAsync(output, next, ct);
        }

        throw new NoFinalAnswerReachedException(
            $"The agent did not reach a final answer within {MaxSteps} steps.", MaxSteps, output);
    }

    /// <summary>
    /// Runs the same loop, but streams the final answer as the model writes it. The thoughts, actions
    /// and observations leading up to it are withheld: only what follows "Final Answer:" is the player's
    /// to see. Nothing is yielded until the model gets there, which may take several tool calls.
    /// </summary>
    /// <remarks>The model must have been created for streaming, or its whole answer arrives as one chunk.</remarks>
    /// <exception cref="NoFinalAnswerReachedException">The agent never reached a final answer.</exception>
    public async IAsyncEnumerable<string> RunStreamingAsync(
        string input,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var run = StartRun(input);

        string output = string.Empty;

        for (int step = 0; step < MaxSteps; step++)
        {
            var filter = new FinalAnswerFilter(); // Clear filter in each step to ensure no leaks

            await foreach (var response in model.GenerateAsync(run.NextRequest(), ReActFormat, ct))
            {
                if (response.Delta is not { } delta)
                {
                    // The last response of a call carries the whole message instead of a delta.
                    output = response.LastMessageContent;
                    continue;
                }

                if (filter.Consume(delta.Content) is { Length: > 0 } text)
                {
                    yield return text;
                }
            }

            var next = ReActOutputParser.Parse(output);
            if (next is ReActStep.FinalAnswer)
            {
                yield break; // Already streamed above, so do nothing.
            }

            await run.RecordAsync(output, next, ct);
        }

        throw new NoFinalAnswerReachedException(
            $"The agent did not reach a final answer within {MaxSteps} steps.", MaxSteps, output);
    }

    private Run StartRun(string input)
    {
        var variables = new Dictionary<string, string>(Variables, StringComparer.Ordinal);
        var toolbox = new ReActToolbox(Tools);

        variables["input"] = input;
        variables["tools"] = toolbox.Descriptions;
        variables["tool_names"] = toolbox.Names;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxSteps);

        return new Run(new PromptTemplate(prompt), toolbox, variables);
    }

    /// <summary>The state one run of the loop carries from step to step.</summary>
    private sealed class Run(PromptTemplate template, ReActToolbox toolbox, Dictionary<string, string> variables)
    {
        private readonly ReActScratchpad _scratchpad = new();

        public ChatRequest NextRequest()
        {
            variables["history"] = _scratchpad.ToString();

            return ChatRequest.ToChatRequest(template.Render(variables));
        }

        /// <summary>Runs whatever the model asked for and writes the result to the scratchpad.</summary>
        public async Task RecordAsync(string output, ReActStep step, CancellationToken ct)
        {
            string observation = step switch
            {
                ReActStep.UseTool useTool => await toolbox.ObserveAsync(useTool.Name, useTool.Input, ct),
                _ => FormatReminder,
            };

            _scratchpad.Record(output, observation);
        }
    }
}
