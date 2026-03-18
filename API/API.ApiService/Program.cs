using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// JWT configuration (for now – simple defaults, override via appsettings)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev_super_secret_key_123!dev_super_secret_key_123!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CreativityApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CreativityApiClient";
var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Weather forecast demo (original template)
string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Mock users storage - in-memory only
var mockUsers = new List<User>
{
    new(
        Id: 123,
        Phone: "+79001234567",
        Username: "johndoe",
        DisplayName: "John Doe",
        Avatar: "https://cdn.example.com/avatars/123.jpg",
        Bio: "Hello there!",
        Settings: new UserSettings(Notifications: true, Theme: "dark"),
        LastSeen: DateTimeOffset.Parse("2024-01-01T12:00:00Z")
    ),
    new(
        Id: 456,
        Phone: "+79009876543",
        Username: "friend",
        DisplayName: "Friend Name",
        Avatar: "https://cdn.example.com/avatars/456.jpg",
        Bio: "Their bio",
        Settings: new UserSettings(Notifications: true, Theme: "light"),
        LastSeen: null
    )
};

// Helper to get "current" user from mock store
User GetCurrentUser() => mockUsers.First();

string GenerateJwtToken(User user)
{
    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.PhoneNumber, user.Phone),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var signingKey = new SymmetricSecurityKey(jwtKeyBytes);
    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// 1. Users – Registration & Profile Management

// POST /auth/register
app.MapPost("/auth/register", (RegisterRequest request) =>
{
    var newUserId = mockUsers.Max(u => u.Id) + 1;

    var user = new User(
        Id: newUserId,
        Phone: request.Phone,
        Username: string.IsNullOrWhiteSpace(request.Username) ? $"user{newUserId}" : request.Username,
        DisplayName: request.DisplayName,
        Avatar: null,
        Bio: null,
        Settings: new UserSettings(Notifications: true, Theme: "dark"),
        LastSeen: DateTimeOffset.UtcNow
    );

    mockUsers.Add(user);

    var token = GenerateJwtToken(user);
    var response = new AuthRegisterResponse(user, Token: token);
    return Results.Created($"/users/{user.Id}", response);
});

// POST /auth/verify
app.MapPost("/auth/verify", (AuthVerifyRequest request) =>
{
    var user = mockUsers.FirstOrDefault(u => u.Phone == request.Phone) ?? GetCurrentUser();

    var token = GenerateJwtToken(user);
    var response = new AuthVerifyResponse(
        User: user,
        Token: token,
        Settings: new { }
    );
    return Results.Ok(response);
});

// POST /auth/refresh
app.MapPost("/auth/refresh", (AuthRefreshRequest request) =>
{
    // In this mock implementation we don't really validate/rotate refresh tokens,
    // just issue a new access token for the same "current" user.
    var user = GetCurrentUser();
    var newAccessToken = GenerateJwtToken(user);
    var response = new AuthRefreshResponse(AccessToken: newAccessToken);
    return Results.Ok(response);
});

// GET /users/me
app.MapGet("/users/me", () =>
{
    var user = GetCurrentUser();
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

// PATCH /users/me
app.MapPatch("/users/me", (UpdateProfileRequest request) =>
{
    var user = GetCurrentUser();
    var updatedUser = user with
    {
        DisplayName = request.DisplayName ?? user.DisplayName,
        Username = request.Username ?? user.Username,
        Bio = request.Bio ?? user.Bio,
        Avatar = request.Avatar ?? user.Avatar
    };

    var index = mockUsers.FindIndex(u => u.Id == user.Id);
    if (index >= 0)
    {
        mockUsers[index] = updatedUser;
    }

    return Results.NoContent();
}).RequireAuthorization();

// POST /users/me/avatar
app.MapPost("/users/me/avatar", async (HttpRequest httpRequest) =>
{
    var form = await httpRequest.ReadFormAsync();
    var avatarFile = form.Files["avatar"];

    // In this mock implementation we don't store file anywhere,
    // just return a sample URL.
    var avatarUrl = "https://cdn.example.com/avatars/123.jpg";

    var response = new { avatar_url = avatarUrl };
    return Results.Ok(response);
}).RequireAuthorization();

// GET /users/:id
app.MapGet("/users/{id:int}", (int id) =>
{
    var user = mockUsers.FirstOrDefault(u => u.Id == id);
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

// GET /users/search
app.MapGet("/users/search", (string q, int? limit) =>
{
    var max = Math.Clamp(limit ?? 20, 1, 100);

    var filtered = mockUsers
        .Where(u =>
            (!string.IsNullOrEmpty(u.Username) && u.Username.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(u.DisplayName) && u.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)))
        .Take(max)
        .Select(u => new UserSearchItem(
            Id: u.Id,
            Username: u.Username,
            DisplayName: u.DisplayName,
            Avatar: u.Avatar
        ))
        .ToList();

    var response = new UsersSearchResponse(
        Users: filtered,
        TotalCount: filtered.Count
    );

    return Results.Ok(response);
});

// POST /users/me/logout
app.MapPost("/users/me/logout", () =>
{
    var response = new { success = true };
    return Results.Ok(response);
}).RequireAuthorization();

app.MapDefaultEndpoints();

app.Run();

// DTOs and models for Users API

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record User(
    int Id,
    string Phone,
    string? Username,
    string DisplayName,
    string? Avatar,
    string? Bio,
    UserSettings Settings,
    DateTimeOffset? LastSeen
);

record UserSettings(bool Notifications, string Theme);

record RegisterRequest(string Phone, string DisplayName, string? Username);

record AuthRegisterResponse(User User, string Token);

record AuthVerifyRequest(string Phone, string Code);

record AuthVerifyResponse(User User, string Token, object Settings);

record AuthRefreshRequest(string RefreshToken);

record AuthRefreshResponse(string AccessToken);

record CurrentUserResponse(
    int Id,
    string Phone,
    string? Username,
    string DisplayName,
    string? Avatar,
    string Bio,
    object Settings,
    DateTimeOffset LastSeen
);

record UpdateProfileRequest(
    string? DisplayName,
    string? Username,
    string? Bio,
    string? Avatar
);

record OtherUserProfileResponse(
    int Id,
    string? Username,
    string DisplayName,
    string? Avatar,
    string Bio,
    bool IsOnline,
    DateTimeOffset? LastSeen
);

record UserSearchItem(
    int Id,
    string? Username,
    string DisplayName,
    string? Avatar
);

record UsersSearchResponse(
    IReadOnlyCollection<UserSearchItem> Users,
    int TotalCount
);
