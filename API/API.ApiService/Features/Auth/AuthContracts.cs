using API.Domain.Users;

namespace API.ApiService.Features.Auth;

public record RegisterRequest(string Phone, string DisplayName, string? Username);

public record AuthRegisterResponse(User User, string Token);

public record AuthVerifyRequest(string Phone, string Code);

public record AuthVerifyResponse(User User, string Token, object Settings);

public record AuthRefreshRequest(string RefreshToken);

public record AuthRefreshResponse(string AccessToken);
