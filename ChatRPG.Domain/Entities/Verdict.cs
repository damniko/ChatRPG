using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class Verdict : IEntity
{
    private Verdict()
    {
    }

    public Verdict(Campaign campaign, string content)
    {
        Campaign = campaign;
        Content = content;
    }

    public int Id { get; init; }
    public Campaign Campaign { get; private set; } = null!;
    public string Content { get; private set; } = null!;
}
