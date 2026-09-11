using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IUserDirectory
{
    Task<User> GetOrCreateAsync(Guid identityId, string username, CancellationToken ct = default);
}
