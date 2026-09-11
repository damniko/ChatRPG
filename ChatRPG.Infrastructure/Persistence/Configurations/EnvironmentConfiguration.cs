using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Environment = ChatRPG.Domain.Entities.Environment;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class EnvironmentConfiguration : IEntityTypeConfiguration<Environment>
{
    public void Configure(EntityTypeBuilder<Environment> builder)
    {
        builder.ToTable("Environments");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Campaign).WithMany(c => c.Environments);
    }
}
