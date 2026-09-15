using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class Location : IEntity
{
    private Location() { }

    public Location(Campaign campaign, string name, string description)
    {
        Campaign = campaign;
        Name = name;
        Description = description;
    }

    public int Id { get; init; }
    public Campaign Campaign { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Description { get; set; } = null!;
}
