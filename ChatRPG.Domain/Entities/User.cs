using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class User : IEntity
{
    public int Id { get; init; }
    public required Guid IdentityId { get; init; }
    public required string Username { get; set; }
    public virtual ICollection<Campaign> Campaigns { get; } = new List<Campaign>();
}
