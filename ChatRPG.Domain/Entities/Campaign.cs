using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class Campaign(User user, string title) : IEntity
{
    public Campaign(User user, string title, string startScenario, bool isOpenWorld) : this(user, title)
    {
        StartScenario = startScenario;
        IsOpenWorld = isOpenWorld;
    }

    public int Id { get; init; }
    public string? StartScenario { get; init; }
    public User User { get; private set; } = user;
    public string Title { get; private set; } = title;
    public DateTime StartedOn { get; private set; } = DateTime.UtcNow;
    public ICollection<Message> Messages { get; } = new List<Message>();
    public string GameSummary { get; set; } = string.Empty;
    public ICollection<Character> Characters { get; } = new List<Character>();
    public ICollection<Environment> Environments { get; } = new List<Environment>();
    public Character Player => Characters.First(c => c.IsPlayer);
    public bool IsOpenWorld { get; init; }
    public NarrativeGraph? NarrativeGraph { get; init; }
    public bool GameOver { get; set; }
    public bool IsSnapshot { get; init; }
}
