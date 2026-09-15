using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Gameplay;

public abstract record NarrationRequest(Campaign Campaign)
{
    /// <summary>The first narration of a campaign, from its starting scenario.</summary>
    public sealed record Opening(Campaign Campaign, string Scenario, string? GraphSummary = null)
        : NarrationRequest(Campaign);

    /// <summary>The player acted. <paramref name="Ruling" /> is null for open-world campaigns.</summary>
    public sealed record PlayerTurn(
        Campaign Campaign,
        PlayerAction Action,
        ActionRuling? Ruling = null,
        string? GraphSummary = null) : NarrationRequest(Campaign);

    /// <summary>
    /// The campaign has ended; narrate the closing scene after <paramref name="PrecedingNarration" />.
    /// <paramref name="Action" /> is the turn that ended it.
    /// </summary>
    public sealed record Epilogue(Campaign Campaign, PlayerAction Action, string PrecedingNarration)
        : NarrationRequest(Campaign);
}
