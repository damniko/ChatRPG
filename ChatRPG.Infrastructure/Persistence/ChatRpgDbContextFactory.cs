using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatRPG.Infrastructure.Persistence;

public sealed class ChatRpgDbContextFactory : IDesignTimeDbContextFactory<ChatRpgDbContext>
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=chatrpg;Username=postgres;Password=postgres";

    public ChatRpgDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("CHATRPG_DB_CONNECTION") ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<ChatRpgDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ChatRpgDbContext(options);
    }
}
