using System.Text.Json.Serialization;

namespace API.Contracts.Users;

public sealed record UserSettingsResponse(bool Notifications, string Theme);

public sealed record CurrentUserResponse(
    int Id,
    string Phone,
    string? Username,
    string DisplayName,
    string? Avatar,
    string Bio,
    UserSettingsResponse Settings,
    DateTimeOffset LastSeen
);

public sealed record UpdateProfileRequest(
    string? DisplayName,
    string? Username,
    string? Bio,
    string? Avatar
);

public sealed record OtherUserProfileResponse(
    int Id,
    string? Username,
    string DisplayName,
    string? Avatar,
    string Bio,
    bool IsOnline,
    DateTimeOffset? LastSeen
);

public sealed record UserSearchItem(
    int Id,
    string? Username,
    string DisplayName,
    string? Avatar
);

public sealed record UsersSearchResponse(
    IReadOnlyCollection<UserSearchItem> Users,
    int TotalCount
);

public sealed record AvatarUploadResponse(
    [property: JsonPropertyName("avatar_url")] string AvatarUrl
);

public sealed record LogoutResponse(bool Success);
