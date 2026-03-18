using AppAuthService = API.Application.Auth.AuthService;

namespace API.ApiService.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/register", (RegisterRequest request, AppAuthService authService) =>
        {
            var (user, token) = authService.Register(request.Phone, request.DisplayName, request.Username);
            var response = new AuthRegisterResponse(user, token);
            return Results.Created($"/users/{response.User.Id}", response);
        });

        auth.MapPost("/verify", (AuthVerifyRequest request, AppAuthService authService) =>
        {
            var (user, token) = authService.Verify(request.Phone);
            var response = new AuthVerifyResponse(User: user, Token: token, Settings: new { });
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
}
