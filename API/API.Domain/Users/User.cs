namespace API.Domain.Users;

public record User(
    int Id,
    string Phone,
    string? Username,
    string DisplayName,
    string? Avatar,
    string? Bio,
    UserSettings Settings,
    DateTimeOffset? LastSeen,
    string PasswordHash
);

public record UserSettings(bool Notifications, string Theme);
