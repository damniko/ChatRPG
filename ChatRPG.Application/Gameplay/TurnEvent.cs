using ChatRPG.Application.Usage;

namespace ChatRPG.Application.Gameplay;

public abstract record TurnEvent
{
    public sealed record InputRejected(string Reasoning) : TurnEvent;
    public sealed record NarrationStarted : TurnEvent;
    public sealed record NarrationChunk(string Text) : TurnEvent;
    public sealed record NarrationCompleted(string FullText) : TurnEvent;
    public sealed record ArchivingStarted : TurnEvent;
    public sealed record GameEnded(string Epilogue) : TurnEvent;
    public sealed record CampaignSaved : TurnEvent;

    /// <summary>
    /// What the turn has cost so far. Carried on the turn stream rather than raised as an event so
    /// that any host can see it: the stream maps onto a Blazor render, a server-sent event, or a
    /// console write without the turn service knowing which it is.
    /// </summary>
    public sealed record UsageUpdated(LlmUsageSnapshot Usage) : TurnEvent;
}
