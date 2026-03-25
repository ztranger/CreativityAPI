using AppAuthService = API.Application.Auth.AuthService;
using API.Contracts.Auth;

namespace API.ApiService.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/register", (RegisterRequest request, AppAuthService authService) =>
        {
            var (user, token) = authService.Register(request.Phone, request.DisplayName, request.Username);
            var response = new AuthRegisterResponse(ToAuthUserResponse(user), token);
            return Results.Created($"/users/{response.User.Id}", response);
        });

        auth.MapPost("/verify", (AuthVerifyRequest request, AppAuthService authService) =>
        {
            var (user, token) = authService.Verify(request.Phone);
            var settings = new AuthUserSettingsResponse(user.Settings.Notifications, user.Settings.Theme);
            var response = new AuthVerifyResponse(User: ToAuthUserResponse(user), Token: token, Settings: settings);
            return Results.Ok(response);
        });

        auth.MapPost("/refresh", (AuthRefreshRequest request, AppAuthService authService) =>
        {
            _ = request;
            var response = new AuthRefreshResponse(authService.RefreshAccessToken());
            return Results.Ok(response);
        });

        return app;
    }

    private static AuthUserResponse ToAuthUserResponse(API.Domain.Users.User user)
    {
        var settings = new AuthUserSettingsResponse(user.Settings.Notifications, user.Settings.Theme);
        return new AuthUserResponse(
            Id: user.Id,
            Phone: user.Phone,
            Username: user.Username,
            DisplayName: user.DisplayName,
            Avatar: user.Avatar,
            Bio: user.Bio,
            Settings: settings,
            LastSeen: user.LastSeen
        );
    }
}
