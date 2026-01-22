using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage.InMemory;

public sealed class InMemoryBalanceStore(InMemoryState state) : IBalanceStore
{
    public Task<long> GetAsync(Guid userId, CancellationToken ct)
    {
        if (!state.UsersById.TryGetValue(userId, out var user) || !user.IsActive)
            throw new InvalidOperationException("User not found or inactive.");

        return Task.FromResult(user.Balance);
    }

    public Task<long> AddAsync(Guid userId, long amount, CancellationToken ct) => ApplyAsync(userId, amount);
    public Task<long> SubtractAsync(Guid userId, long amount, CancellationToken ct) => ApplyAsync(userId, -amount);

    private Task<long> ApplyAsync(Guid userId, long change)
    {
        if (!state.UsersById.TryGetValue(userId, out var user) || !user.IsActive)
            throw new InvalidOperationException("User not found or inactive.");

        lock (user)
        {
            checked
            {
                var next = user.Balance + change;
                if (next < 0) throw new InvalidOperationException("Insufficient balance.");
                user.Balance = next;
                return Task.FromResult(user.Balance);
            }
        }
    }
}