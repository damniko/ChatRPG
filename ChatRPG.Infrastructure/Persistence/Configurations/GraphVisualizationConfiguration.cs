using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class GraphVisualizationConfiguration : IEntityTypeConfiguration<GraphVisualization>
{
    public void Configure(EntityTypeBuilder<GraphVisualization> builder)
    {
        builder.ToTable("GraphVisualizations");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.NarrativeGraph).WithMany(x => x.Visualizations);
    }
}