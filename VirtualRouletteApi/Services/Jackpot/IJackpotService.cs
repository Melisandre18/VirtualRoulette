using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Jackpot;

public interface IJackpotService
{
    Task<JackpotResponse> GetAsync(CancellationToken ct);
}