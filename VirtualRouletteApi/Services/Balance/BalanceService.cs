using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Services.Balance;

public class BalanceService(IBalanceStore store) : IBalanceService
{
    public async Task<BalanceResponse> GetAsync(Guid userId, CancellationToken ct)
    {
        var balance = await store.GetAsync(userId, ct);
        return new BalanceResponse(balance);
    }

    public async Task<BalanceResponse> DepositAsync(Guid userId, long amount, CancellationToken ct)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        var updated = await store.AddAsync(userId, amount, ct);
        return new BalanceResponse(updated);
    }

    public async Task<BalanceResponse> WithdrawAsync(Guid userId, long amount, CancellationToken ct)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        var updated = await store.SubtractAsync(userId, amount, ct);
        return new BalanceResponse(updated);
    }
}