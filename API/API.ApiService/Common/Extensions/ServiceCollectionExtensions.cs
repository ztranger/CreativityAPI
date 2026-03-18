using System.Text;
using API.Application.Auth;
using API.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.ApiService.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = new JwtOptions
        {
            Key = configuration["Jwt:Key"] ?? "dev_super_secret_key_123!dev_super_secret_key_123!",
            Issuer = configuration["Jwt:Issuer"] ?? "CreativityApi",
            Audience = configuration["Jwt:Audience"] ?? "CreativityApiClient"
        };

        var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);

        services.AddSingleton(jwtOptions);
        services.AddSingleton<IUserTokenService, JwtTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
