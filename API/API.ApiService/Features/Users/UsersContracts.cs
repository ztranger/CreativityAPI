namespace API.ApiService.Features.Users;

public record CurrentUserResponse(
    int Id,
    string Phone,
    string? Username,
    string DisplayName,
    string? Avatar,
    string Bio,
    object Settings,
    DateTimeOffset LastSeen
);

public record UpdateProfileRequest(
    string? DisplayName,
    string? Username,
    string? Bio,
    string? Avatar
);

public record OtherUserProfileResponse(
    int Id,
    string? Username,
    string DisplayName,
    string? Avatar,
    string Bio,
    bool IsOnline,
    DateTimeOffset? LastSeen
);

public record UserSearchItem(
    int Id,
    string? Username,
    string DisplayName,
    string? Avatar
);

public record UsersSearchResponse(
    IReadOnlyCollection<UserSearchItem> Users,
    int TotalCount
);
