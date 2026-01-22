using VirtualRouletteApi.Infrastructure.Storage.InMemory;
using VirtualRouletteApi.Infrastructure.Storage.Postgres;

namespace VirtualRouletteApi.Infrastructure.Storage;

public static class StorageRegistration
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration config)
    {
        var provider = (config["Storage:Provider"] ?? "Postgres").Trim();

        switch (provider)
        {
            case "Postgres":
                services.AddScoped<IUserStore, PostgresUserStore>();
                services.AddScoped<IBalanceStore, PostgresBalanceStore>();
                services.AddScoped<IBetStore, PostgresBetStore>();
                services.AddScoped<IJackpotStore, PostgresJackpotStore>();
                services.AddScoped<ISessionStore, PostgresSessionStore>();
                break;

            case "InMemory":
                services.AddSingleton<InMemoryState>();
                services.AddSingleton<IUserStore, InMemoryUserStore>();
                services.AddSingleton<IBalanceStore, InMemoryBalanceStore>();
                services.AddSingleton<IBetStore, InMemoryBetStore>();
                services.AddSingleton<IJackpotStore, InMemoryJackpotStore>();
                services.AddSingleton<ISessionStore, InMemorySessionStore>();
                break;

            case "Redis":
                throw new NotImplementedException("Redis provider not wired.");

            default:
                throw new InvalidOperationException($"Unknown Storage:Provider '{provider}'.");
        }

        return services;
    }
}