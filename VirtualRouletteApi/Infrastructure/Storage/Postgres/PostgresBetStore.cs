using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage.Postgres;

public sealed class PostgresBetStore(AppDbContext db) : IBetStore
{
    public Task AddAsync(Bet bet, CancellationToken ct)
    {
        db.Bets.Add(bet);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Bet>> GetHistoryAsync(Guid userId, int take, CancellationToken ct)
    {
        return await db.Bets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}