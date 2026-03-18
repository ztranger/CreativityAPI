using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class ChatMemberEntityConfiguration : IEntityTypeConfiguration<ChatMemberEntity>
{
    public void Configure(EntityTypeBuilder<ChatMemberEntity> builder)
    {
        builder.ToTable("chat_members", tableBuilder =>
            tableBuilder.HasCheckConstraint("CK_chat_members_role", "role IN (1, 2, 3)"));

        builder.HasKey(x => new { x.ChatId, x.UserId });

        builder.Property(x => x.ChatId)
            .HasColumnName("chat_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasDefaultValue((short)1)
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .HasColumnName("joined_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.LeftAt)
            .HasColumnName("left_at");

        builder.Property(x => x.MuteUntil)
            .HasColumnName("mute_until");

        builder.Property(x => x.LastReadMessageId)
            .HasColumnName("last_read_message_id");

        builder.Property(x => x.IsPinned)
            .HasColumnName("is_pinned")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne<ChatEntity>()
            .WithMany()
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.ChatId });
        builder.HasIndex(x => new { x.ChatId, x.UserId });
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_chat_members_user_id_active")
            .HasFilter("left_at IS NULL");
    }
}
