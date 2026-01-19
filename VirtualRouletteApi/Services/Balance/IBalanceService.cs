using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Balance;

public interface IBalanceService
{
    Task<BalanceResponse> GetAsync(Guid userId, CancellationToken ct);
    Task<BalanceResponse> DepositAsync(Guid userId, long amount, CancellationToken ct);
    Task<BalanceResponse> WithdrawAsync(Guid userId, long amount, CancellationToken ct);
}