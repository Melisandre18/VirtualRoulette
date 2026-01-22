using VirtualRouletteApi.Domain;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Infrastructure.Storage;
using VirtualRouletteApi.Services.Jackpot;
using VirtualRouletteApi.Services.Roulette;

namespace VirtualRouletteApi.Services.Bets;

public class BetService(
    IUserStore users,
    IBalanceStore balances,
    IBetStore bets,
    IRouletteService roulette,
    IJackpotService jackpot
) : IBetService
{
    public async Task<BetResponse> MakeBetAsync(Guid userId, string betJson, string ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(betJson) || !roulette.ValidateBet(betJson, out var betAmount))
            return new BetResponse("rejected", Guid.Empty, 0, 0);
        var user = await users.FindByIdAsync(userId, ct);
        if (user is null || !user.IsActive)
            return new BetResponse("rejected", Guid.Empty, 0, 0);

        var currentBalance = await balances.GetAsync(userId, ct);
        if (currentBalance < betAmount)
            return new BetResponse("rejected", Guid.Empty, 0, 0);
        
        await balances.SubtractAsync(userId, betAmount, ct);
        
        await jackpot.ChangeOnBetAsync(betAmount, ct);
        
        var winningNumber = roulette.GenerateWinningNumber();
        var winAmount = roulette.CalculateWin(betJson, winningNumber);
        
        if (winAmount > 0)
            await balances.AddAsync(userId, winAmount, ct);


        var bet = new Bet
        {
            UserId = userId,
            BetJson = betJson,
            BetAmount = betAmount,
            WinningNumber = winningNumber,
            WinAmount = winAmount,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await bets.AddAsync(bet, ct);
        await users.SaveAsync(ct);

        return new BetResponse("accepted", bet.Id, winningNumber, winAmount);
    }

    public async Task<IReadOnlyList<BetHistory>> GetHistoryAsync(Guid userId, int take, CancellationToken ct)
    {
        if (take <= 0) take = 10;
        if (take > 100) take = 100;

        var history = await bets.GetHistoryAsync(userId, take, ct);

        return history
            .OrderByDescending(b => b.CreatedAt)
            .Take(take)
            .Select(b => new BetHistory(
                b.Id,
                b.BetAmount,
                b.WinAmount,
                b.CreatedAt
            ))
            .ToList();
    }
}