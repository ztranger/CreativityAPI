using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class MessageContentEntityConfiguration : IEntityTypeConfiguration<MessageContentEntity>
{
    public void Configure(EntityTypeBuilder<MessageContentEntity> builder)
    {
        builder.ToTable("message_content", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_message_content_not_empty",
                "COALESCE(LENGTH(TRIM(text)), 0) > 0 OR JSONB_ARRAY_LENGTH(emoji_payload) > 0"));

        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .ValueGeneratedNever();

        builder.Property(x => x.Text)
            .HasColumnName("text");

        builder.Property(x => x.Entities)
            .HasColumnName("entities")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();

        builder.Property(x => x.EmojiPayload)
            .HasColumnName("emoji_payload")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();

        builder.HasOne<MessageEntity>()
            .WithOne()
            .HasForeignKey<MessageContentEntity>(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
