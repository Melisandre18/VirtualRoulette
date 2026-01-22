using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;

namespace VirtualRouletteApi.Infrastructure.Storage.Postgres;

public sealed class PostgresSessionStore(AppDbContext db) : ISessionStore
{
    public Task<int> SignOutInactiveAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        return db.Users
            .Where(u => u.IsActive && u.LastSeen < cutoff)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false), ct);
    }
}