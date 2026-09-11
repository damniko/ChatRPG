using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class StartScenarioConfiguration : IEntityTypeConfiguration<StartScenario>
{
    public void Configure(EntityTypeBuilder<StartScenario> builder)
    {
        builder.ToTable("StartScenarios");
        builder.HasKey(n => n.Id);
        builder.HasMany(s => s.Campaigns);
    }
}
