using System.Diagnostics.CodeAnalysis;

namespace ChatRPG.Domain.Entities;

public sealed class PlayerMessage : Message
{
    private PlayerMessage() { }

    [SetsRequiredMembers]
    public PlayerMessage(Campaign campaign, string content, ActionRuling? ruling = null)
        : base(campaign, content)
    {
        Ruling = ruling;
    }

    /// <summary>Null for open-world campaigns, which are never examined.</summary>
    public ActionRuling? Ruling { get; init; }
}
