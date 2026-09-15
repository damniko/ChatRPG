using System.Diagnostics.CodeAnalysis;

namespace ChatRPG.Domain.Entities;

public sealed class NarrationMessage : Message
{
    private NarrationMessage() { }

    [SetsRequiredMembers]
    public NarrationMessage(Campaign campaign, string content) : base(campaign, content) { }
}
