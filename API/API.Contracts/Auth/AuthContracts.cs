namespace API.Contracts.Auth;

public sealed record RegisterRequest
{
    public string Phone { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? Username { get; init; }

    public string? Password { get; init; }
}

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

public sealed record LoginRequest(string Phone, string Password);

public sealed record LoginResponse(AuthUserResponse User, string Token);

public sealed record AuthVerifyRequest(string Phone, string Code);

public sealed record AuthVerifyResponse(AuthUserResponse User, string Token, AuthUserSettingsResponse Settings);

public sealed record AuthRefreshRequest(string RefreshToken);

public sealed record AuthRefreshResponse(string AccessToken);
