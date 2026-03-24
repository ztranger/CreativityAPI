using API.Application.Chats;
using API.Infrastructure.Chats;

namespace API.ApiService.Features.Chats;

public static class ChatsFeatureRegistration
{
    private const string InMemoryProvider = "InMemory";
    private const string PostgresProvider = "Postgres";

    public static IServiceCollection AddChatsFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["UsersRepository:Provider"] ?? InMemoryProvider;
        if (string.Equals(provider, PostgresProvider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IChatsRepository, PostgresChatsRepository>();
            services.AddScoped<IMessagesRepository, PostgresMessagesRepository>();
        }
        else if (string.Equals(provider, InMemoryProvider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<InMemoryMessagingStore>();
            services.AddSingleton<IChatsRepository, InMemoryChatsRepository>();
            services.AddSingleton<IMessagesRepository, InMemoryMessagesRepository>();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported chats repository provider '{provider}'. Supported values: {InMemoryProvider}, {PostgresProvider}.");
        }

        services.AddScoped<ChatsService>();

        return services;
    }
}
