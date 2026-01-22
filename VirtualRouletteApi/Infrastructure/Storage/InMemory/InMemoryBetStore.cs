using System.Collections.Concurrent;
using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage.InMemory;

public sealed class InMemoryBetStore(InMemoryState state) : IBetStore
{
    public Task AddAsync(Bet bet, CancellationToken ct)
    {
        var q = state.BetsByUser.GetOrAdd(bet.UserId, _ => new ConcurrentQueue<Bet>());
        q.Enqueue(bet);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Bet>> GetHistoryAsync(Guid userId, int take, CancellationToken ct)
    {
        if (!state.BetsByUser.TryGetValue(userId, out var q))
            return Task.FromResult<IReadOnlyList<Bet>>(Array.Empty<Bet>());
        
        var list = q.ToArray()
            .OrderByDescending(b => b.CreatedAt)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<Bet>>(list);
    }
}