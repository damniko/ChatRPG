using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        builder.HasKey(c => c.Id);
        builder.HasMany(c => c.Messages).WithOne(m => m.Campaign);
        builder.HasMany(c => c.Characters).WithOne(ch => ch.Campaign);
        builder.HasMany(c => c.Locations).WithOne(e => e.Campaign);
        builder.HasOne(c => c.NarrativeGraph).WithMany(g => g.Campaigns);
    }
}
