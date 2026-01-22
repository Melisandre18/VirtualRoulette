using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;

namespace VirtualRouletteApi.Infrastructure.Storage.Postgres;

public sealed class PostgresBalanceStore(AppDbContext db) : IBalanceStore
{
    public async Task<long> GetAsync(Guid userId, CancellationToken ct)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => u.Balance)
            .SingleAsync(ct);
    }

    public Task<long> AddAsync(Guid userId, long amount, CancellationToken ct) =>
        ApplyAsync(userId, amount, ct);

    public Task<long> SubtractAsync(Guid userId, long amount, CancellationToken ct) =>
        ApplyAsync(userId, -amount, ct);

    private async Task<long> ApplyAsync(Guid userId, long amount, CancellationToken ct)
    {
        var user = await db.Users.SingleAsync(u => u.Id == userId && u.IsActive, ct);

        checked
        {
            var next = user.Balance + amount;
            if (next < 0) throw new InvalidOperationException("Insufficient balance.");
            user.Balance = next;
        }

        await db.SaveChangesAsync(ct);
        return user.Balance;
    }
}