using API.Application.Users;
using API.Infrastructure.Persistence;
using API.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace API.ApiService.Features.Users;

public static class UsersFeatureRegistration
{
    private const string InMemoryProvider = "InMemory";
    private const string PostgresProvider = "Postgres";

    public static IServiceCollection AddUsersFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["UsersRepository:Provider"] ?? InMemoryProvider;
        if (string.Equals(provider, PostgresProvider, StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("Main");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'Main' is required when UsersRepository:Provider is set to Postgres.");
            }

            services.AddDbContext<ApiDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IUsersRepository, PostgresUsersRepository>();
        }
        else if (string.Equals(provider, InMemoryProvider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IUsersRepository, InMemoryUsersRepository>();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported users repository provider '{provider}'. Supported values: {InMemoryProvider}, {PostgresProvider}.");
        }

        services.AddScoped<UsersService>();

        return services;
    }
}
