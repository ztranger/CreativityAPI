using System.Text.Json;
using API.Domain.Users;
using API.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        var settingsConverter = new ValueConverter<UserSettings, string>(
            value => JsonSerializer.Serialize(value, JsonSerializerOptions),
            value => JsonSerializer.Deserialize<UserSettings>(value, JsonSerializerOptions) ?? new UserSettings(true, "dark"));

        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasMaxLength(50);

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AvatarUrl)
            .HasColumnName("avatar_url");

        builder.Property(x => x.Bio)
            .HasColumnName("bio");

        builder.Property(x => x.LastSeenAt)
            .HasColumnName("last_seen_at");

        builder.Property(x => x.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb")
            .HasConversion(settingsConverter)
            .HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(x => x.Phone)
            .IsUnique();

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.HasIndex(x => x.DisplayName);
    }
}
