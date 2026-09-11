using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class NarrativeGraphConfiguration : IEntityTypeConfiguration<NarrativeGraph>
{
    public void Configure(EntityTypeBuilder<NarrativeGraph> builder)
    {
        builder.ToTable("NarrativeGraphs");
        builder.HasKey(n => n.Id);
        builder.HasMany(g => g.Campaigns).WithOne(c => c.NarrativeGraph);
        builder.HasMany(g => g.Nodes).WithOne(n => n.Graph);
    }
}
