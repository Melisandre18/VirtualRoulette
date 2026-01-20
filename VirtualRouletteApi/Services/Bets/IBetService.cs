using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Bets;

public interface IBetService
{
    Task<BetResponse> MakeBetAsync(Guid userId, string betJson, string ipAddress, CancellationToken ct);
}