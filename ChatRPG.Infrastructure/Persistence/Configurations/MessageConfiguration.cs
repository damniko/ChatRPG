using ChatRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRPG.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);
        builder.HasOne(m => m.Campaign).WithMany(c => c.Messages);
        builder.HasDiscriminator<string>("Author")
            .HasValue<PlayerMessage>("Player")
            .HasValue<NarrationMessage>("Narrator");
    }
}
