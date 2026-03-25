using API.Domain.Users;

namespace API.Infrastructure.Persistence.Entities;

public sealed class UserEntity
{
    public int Id { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Username { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public UserSettings Settings { get; set; } = new(true, "dark");

    public DateTimeOffset? LastSeenAt { get; set; }

    public string PasswordHash { get; set; } = string.Empty;
}
