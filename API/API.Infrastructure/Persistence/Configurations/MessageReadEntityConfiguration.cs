using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class MessageReadEntityConfiguration : IEntityTypeConfiguration<MessageReadEntity>
{
    public void Configure(EntityTypeBuilder<MessageReadEntity> builder)
    {
        builder.ToTable("message_reads");

        builder.HasKey(x => new { x.MessageId, x.UserId });

        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne<MessageEntity>()
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.ReadAt })
            .IsDescending(false, true);
        builder.HasIndex(x => x.MessageId);
    }
}
