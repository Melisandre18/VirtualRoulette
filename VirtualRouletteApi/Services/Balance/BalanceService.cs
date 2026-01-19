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
}
