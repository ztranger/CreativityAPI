using AppUsersService = API.Application.Users.UsersService;

namespace API.ApiService.Features.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users");

        users.MapGet("/me", (AppUsersService usersService) =>
        {
            var user = usersService.GetCurrentUser();
            var response = new CurrentUserResponse(
                Id: user.Id,
                Phone: user.Phone,
                Username: user.Username,
                DisplayName: user.DisplayName,
                Avatar: user.Avatar,
                Bio: user.Bio ?? "Hello there!",
                Settings: new
                {
                    notifications = user.Settings.Notifications,
                    theme = user.Settings.Theme
                },
                LastSeen: user.LastSeen ?? DateTimeOffset.UtcNow
            );
            return Results.Ok(response);
        }).RequireAuthorization();

        users.MapPatch("/me", (UpdateProfileRequest request, AppUsersService usersService) =>
        {
            usersService.UpdateCurrentUser(request.DisplayName, request.Username, request.Bio, request.Avatar);
            return Results.NoContent();
        }).RequireAuthorization();

        users.MapPost("/me/avatar", async (HttpRequest httpRequest) =>
        {
            var form = await httpRequest.ReadFormAsync();
            _ = form.Files["avatar"];
            var avatarUrl = "https://cdn.example.com/avatars/123.jpg";
            var response = new { avatar_url = avatarUrl };
            return Results.Ok(response);
        }).RequireAuthorization();

        users.MapGet("/{id:int}", (int id, AppUsersService usersService) =>
        {
            var user = usersService.GetOtherUser(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            var response = new OtherUserProfileResponse(
                Id: user.Id,
                Username: user.Username,
                DisplayName: user.DisplayName,
                Avatar: user.Avatar,
                Bio: user.Bio ?? "Their bio",
                IsOnline: true,
                LastSeen: null
            );
            return Results.Ok(response);
        });

        users.MapGet("/search", (string q, int? limit, AppUsersService usersService) =>
        {
            var usersResult = usersService.SearchUsers(q, limit)
                .Select(u => new UserSearchItem(
                    Id: u.Id,
                    Username: u.Username,
                    DisplayName: u.DisplayName,
                    Avatar: u.Avatar
                ))
                .ToList();

            var response = new UsersSearchResponse(usersResult, usersResult.Count);
            return Results.Ok(response);
        });

        users.MapPost("/me/logout", () =>
        {
            var response = new { success = true };
            return Results.Ok(response);
        }).RequireAuthorization();

        return app;
    }
}
