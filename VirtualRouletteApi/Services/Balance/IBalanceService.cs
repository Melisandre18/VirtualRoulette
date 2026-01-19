using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Balance;

public interface IBalanceService
{
    Task<BalanceResponse> GetAsync(Guid userId, CancellationToken ct);
}