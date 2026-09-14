namespace ChatRPG.Application.Usage;

/// <summary>What one completed model call cost.</summary>
/// <remarks>
/// Provider-agnostic on purpose: the agents translate whatever their LLM library reports into this
/// shape, so nothing outside the agents has to know which library produced it.
/// </remarks>
/// <param name="Operation">Which agent or tool made the call, e.g. "Narrator" or "SearchScenarioTool".</param>
/// <param name="Model">The model that served it, e.g. "gpt-4o".</param>
/// <param name="ContextWindowTokens">The model's window, or null when it did not say.</param>
public sealed record LlmCallUsage(
    string Operation,
    string Model,
    int PromptTokens,
    int CompletionTokens,
    int? ContextWindowTokens,
    TimeSpan Duration,
    decimal? CostUsd,
    UsageAccuracy Accuracy)
{
    public int TotalTokens => PromptTokens + CompletionTokens;

    /// <summary>
    /// How much of the window this one call's prompt filled. Never sum this across calls: the window
    /// bounds a single request, so a total over several of them means nothing.
    /// </summary>
    public double? ContextUsedFraction =>
        ContextWindowTokens is > 0 ? (double)PromptTokens / ContextWindowTokens.Value : null;
}

/// <summary>Whether the token counts came from the provider or from counting them ourselves.</summary>
public enum UsageAccuracy
{
    /// <summary>The provider reported the counts.</summary>
    Reported,

    /// <summary>We counted them locally, because the provider did not report them.</summary>
    Estimated,
}
