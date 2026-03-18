using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.ToTable("messages", tableBuilder =>
            tableBuilder.HasCheckConstraint("CK_messages_message_type", "message_type IN (1, 2, 3)"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ChatId)
            .HasColumnName("chat_id")
            .IsRequired();

        builder.Property(x => x.SenderUserId)
            .HasColumnName("sender_user_id")
            .IsRequired();

        builder.Property(x => x.MessageType)
            .HasColumnName("message_type")
            .IsRequired();

        builder.Property(x => x.ReplyToMessageId)
            .HasColumnName("reply_to_message_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.EditedAt)
            .HasColumnName("edited_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsRequired();

        builder.HasOne<ChatEntity>()
            .WithMany()
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MessageEntity>()
            .WithMany()
            .HasForeignKey(x => x.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.ChatId, x.CreatedAt, x.Id })
            .IsDescending(false, true, true);
        builder.HasIndex(x => x.SenderUserId);
        builder.HasIndex(x => x.ReplyToMessageId)
            .HasFilter("reply_to_message_id IS NOT NULL");
        builder.HasIndex(x => new { x.ChatId, x.CreatedAt })
            .HasDatabaseName("IX_messages_chat_id_created_at_not_deleted")
            .HasFilter("deleted_at IS NULL")
            .IsDescending(false, true);
    }
}
