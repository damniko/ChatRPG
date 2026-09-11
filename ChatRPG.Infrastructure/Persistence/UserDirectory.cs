using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatRPG.Infrastructure.Persistence;

public class UserDirectory(ChatRpgDbContext dbContext) : IUserDirectory
{
    public async Task<User> GetOrCreateAsync(Guid identityId, string username, CancellationToken ct = default)
    {
        var user = await dbContext.DomainUsers.Where(u => u.IdentityId == identityId).FirstOrDefaultAsync(ct);
        if (user != null) return user;

        user = new User
        {
            IdentityId = identityId,
            Username = username
        };
        dbContext.Add(user);
        await dbContext.SaveChangesAsync(ct);
        return user;
    }
}
