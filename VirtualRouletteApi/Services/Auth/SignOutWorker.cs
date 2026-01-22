using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Services.Auth;

public class SignOutWorker(IServiceScopeFactory scopes) : BackgroundService
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            using var scope = scopes.CreateScope();
            var sessions = scope.ServiceProvider.GetRequiredService<ISessionStore>();

            var cutoff = DateTimeOffset.UtcNow - Timeout;
            await sessions.SignOutInactiveAsync(cutoff, stoppingToken);
        }
    }
}