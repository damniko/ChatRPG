using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class NarrativeNodeConfiguration : IEntityTypeConfiguration<NarrativeNode>
{
    public void Configure(EntityTypeBuilder<NarrativeNode> builder)
    {
        builder.ToTable("NarrativeNodes");
        builder.HasKey(n => n.Id);
        builder.HasOne(n => n.Graph).WithMany(g => g.Nodes);
    }
}
