using API.ApiService.Common.Extensions;
using API.ApiService.Features.Auth;
using API.ApiService.Features.Users;
using API.ApiService.Features.Weather;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddUsersFeature();
builder.Services.AddAuthFeature();

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

app.MapWeatherEndpoints();
app.MapAuthEndpoints();
app.MapUsersEndpoints();

app.MapDefaultEndpoints();

app.Run();
