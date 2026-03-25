using API.ApiService.Common.Extensions;
using API.ApiService.Features.Auth;
using API.ApiService.Features.Chats;
using API.ApiService.Features.Users;
using API.ApiService.Features.Weather;
using API.Application.Common.Exceptions;
using API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddUsersFeature(builder.Configuration);
builder.Services.AddChatsFeature(builder.Configuration);
builder.Services.AddAuthFeature();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
await app.Services.EnsureUsersStorageInitializedAsync(builder.Configuration);

// Configure the HTTP request pipeline.
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is UserUniqueConstraintViolationException uniqueViolation)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                code = uniqueViolation.Code,
                message = uniqueViolation.Message,
                field = uniqueViolation.Field
            });
            return;
        }

        if (exception is InvalidCredentialsException invalidCredentials)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                code = invalidCredentials.Code,
                message = invalidCredentials.Message
            });
            return;
        }

        await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
    });
});

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapWeatherEndpoints();
app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapChatsEndpoints();

app.MapDefaultEndpoints();

app.Run();
