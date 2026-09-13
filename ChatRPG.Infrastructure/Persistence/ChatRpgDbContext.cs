using ChatRPG.Domain.Entities;
using ChatRPG.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Environment = ChatRPG.Domain.Entities.Environment;

namespace ChatRPG.Infrastructure.Persistence;

public class ChatRpgDbContext(DbContextOptions<ChatRpgDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<GraphVisualization> GraphVisualizations => Set<GraphVisualization>();
    public DbSet<Environment> Environments => Set<Environment>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<NarrativeEdge> NarrativeEdges => Set<NarrativeEdge>();
    public DbSet<NarrativeNode> NarrativeNodes => Set<NarrativeNode>();
    public DbSet<NarrativeGraph> NarrativeGraphs => Set<NarrativeGraph>();
    public DbSet<StartScenario> StartScenarios => Set<StartScenario>();
    public DbSet<Verdict> Verdicts => Set<Verdict>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ChatRpgDbContext).Assembly);
    }
}
