using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class VerdictConfiguration : IEntityTypeConfiguration<Verdict>
{
    public void Configure(EntityTypeBuilder<Verdict> builder)
    {
        builder.ToTable("Verdicts");
        builder.HasKey(v => v.Id);
        builder.HasOne(v => v.Campaign);
    }
}
