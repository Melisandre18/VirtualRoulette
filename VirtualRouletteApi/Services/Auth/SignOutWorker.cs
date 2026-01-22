using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Services.Auth;


/// <summary>
/// Background worker that signs out inactive users.
///
/// Basic auth sends credentials on every request, though a user can "go silent"
/// (no requests) we need to get them as inactive after 5 minutes, 
/// providing a server-side cleanup mechanism independent of request traffic.
/// If LastSeen is older than Timeout, the user is signed out.
/// (LastSeen is updated on authenticated activity)
/// </summary>
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