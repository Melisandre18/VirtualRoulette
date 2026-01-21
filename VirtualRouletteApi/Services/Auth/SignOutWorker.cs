using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;

namespace VirtualRouletteApi.Services.Auth;

public class SignOutWorker(IServiceScopeFactory scopes) : BackgroundService
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Console.WriteLine($"[InactivityWorker] : {DateTimeOffset.UtcNow:O}");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTimeOffset.UtcNow - Timeout;

            await db.Users
                .Where(u => u.IsActive && u.LastSeen < cutoff)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(u => u.IsActive, false),
                    stoppingToken);
        }
    }
}