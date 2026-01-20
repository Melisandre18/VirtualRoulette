using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Domain;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Services.Roulette;

namespace VirtualRouletteApi.Services.Bets;

public class BetService(AppDbContext db, IRouletteService roulette) : IBetService
{
    public async Task<BetResponse> MakeBetAsync(Guid userId, string betJson, string ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(betJson) || !roulette.ValidateBet(betJson, out var betAmount))
            return new BetResponse("rejected", Guid.Empty, 0, 0);
        
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var user = await db.Users.SingleAsync(u => u.Id == userId && u.IsActive, ct);

        if (user.Balance < betAmount)
        {
            await tx.RollbackAsync(ct);
            return new BetResponse("rejected", Guid.Empty, 0, 0);
        }
        
        user.Balance -= betAmount;
        
        var winningNumber = roulette.GenerateWinningNumber();
        var winAmount = roulette.CalculateWin(betJson, winningNumber);
        
        checked
        {
            user.Balance += winAmount;
        }

        var bet = new Bet
        {
            UserId = userId,
            BetJson = betJson,
            BetAmount = betAmount,
            WinningNumber = winningNumber,
            WinAmount = winAmount,
            IpAddress = ipAddress
        };

        db.Bets.Add(bet);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new BetResponse("accepted", bet.Id, winningNumber, winAmount);
    }
    
    public async Task<IReadOnlyList<BetHistory>> GetHistoryAsync(Guid userId, int take, CancellationToken ct)
    {
        if (take <= 0) take = 10;
        if (take > 100) take = 100;

        return await db.Bets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BetHistory(
                b.Id,
                b.BetAmount,
                b.WinAmount,
                b.CreatedAt
            ))
            .Take(take)
            .ToListAsync(ct);
    }
}
