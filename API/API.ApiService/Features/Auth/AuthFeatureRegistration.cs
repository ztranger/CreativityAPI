using API.Application.Auth;

namespace API.ApiService.Features.Auth;

public static class AuthFeatureRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        return services;
    }
}
