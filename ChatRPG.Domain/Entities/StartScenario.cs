using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class StartScenario : IEntity
{
    private StartScenario()
    {
    }

    public StartScenario(string title, string body)
    {
        Title = title;
        Body = body;
    }

    public int Id { get; init; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public ICollection<Campaign> Campaigns { get; } = new List<Campaign>();
}
