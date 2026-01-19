using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Balance;

public class BalanceService(AppDbContext db) : IBalanceService
{
    public async Task<BalanceResponse> GetAsync(Guid userId, CancellationToken ct)
    {
        var balance = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive)
            .Select(u => u.Balance)
            .SingleAsync(ct);

        return new BalanceResponse(balance);
    }
    public Task<BalanceResponse> DepositAsync(Guid userId, long amount, CancellationToken ct)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        return ApplyAmountChangeAsync(userId, amount, ct);
    }

    public Task<BalanceResponse> WithdrawAsync(Guid userId, long amount, CancellationToken ct)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        return ApplyAmountChangeAsync(userId, -amount, ct);
    }
    
    private async Task<BalanceResponse> ApplyAmountChangeAsync(Guid userId, long amount, CancellationToken ct)
    {
        var user = await db.Users.SingleAsync(u => u.Id == userId && u.IsActive, ct);

        checked
        {
            var next = user.Balance + amount;
            if (next < 0)
                throw new InvalidOperationException("Insufficient balance.");

            user.Balance = next;
        }

        await db.SaveChangesAsync(ct);
        return new BalanceResponse(user.Balance);
    }
}
