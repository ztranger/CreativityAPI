namespace API.Contracts.Auth;

public sealed record RegisterRequest(string Phone, string DisplayName, string? Username);

public sealed record AuthUserSettingsResponse(bool Notifications, string Theme);

public sealed record AuthUserResponse(
    int Id,
    string Phone,
    string? Username,
    string DisplayName,
    string? Avatar,
    string? Bio,
    AuthUserSettingsResponse Settings,
    DateTimeOffset? LastSeen);

public sealed record AuthRegisterResponse(AuthUserResponse User, string Token);

public sealed record AuthVerifyRequest(string Phone, string Code);

public sealed record AuthVerifyResponse(AuthUserResponse User, string Token, AuthUserSettingsResponse Settings);

public sealed record AuthRefreshRequest(string RefreshToken);

public sealed record AuthRefreshResponse(string AccessToken);
