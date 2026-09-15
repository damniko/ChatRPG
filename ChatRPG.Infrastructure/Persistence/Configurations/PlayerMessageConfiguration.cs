using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class PlayerMessageConfiguration : IEntityTypeConfiguration<PlayerMessage>
{
    public void Configure(EntityTypeBuilder<PlayerMessage> builder)
    {
        builder.OwnsOne(m => m.Ruling, ruling =>
        {
            ruling.Property(r => r.Permission).HasConversion<string>();
            ruling.Property(r => r.Reasoning);
            ruling.Ignore(r => r.AttemptProceeds);
        });
    }
}
