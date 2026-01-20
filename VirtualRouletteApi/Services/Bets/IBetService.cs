using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Bets;

public interface IBetService
{
    Task<BetResponse> MakeBetAsync(Guid userId, string betJson, string ipAddress, CancellationToken ct);
    Task<IReadOnlyList<BetHistory>> GetHistoryAsync(Guid userId, int take, CancellationToken ct);
}