using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class NarrativeEdgeConfiguration : IEntityTypeConfiguration<NarrativeEdge>
{
    public void Configure(EntityTypeBuilder<NarrativeEdge> builder)
    {
        builder.ToTable("NarrativeEdges");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.SourceNode)
            .WithMany(n => n.Edges)
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.TargetNode)
            .WithMany()
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
