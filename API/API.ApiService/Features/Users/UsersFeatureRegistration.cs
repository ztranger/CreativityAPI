using API.Application.Users;
using API.Infrastructure.Users;

namespace API.ApiService.Features.Users;

public static class UsersFeatureRegistration
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddSingleton<IUsersRepository, InMemoryUsersRepository>();
        services.AddSingleton<UsersService>();

        return services;
    }
}
