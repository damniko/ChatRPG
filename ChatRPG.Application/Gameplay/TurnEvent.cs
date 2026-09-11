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
}
