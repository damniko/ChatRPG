using ChatRPG.Application.Abstractions;

namespace ChatRPG.Infrastructure.Persistence;

public class UnitOfWork(ChatRpgDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
