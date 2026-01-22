namespace VirtualRouletteApi.Infrastructure.Storage;

public interface IJackpotStore
{
    Task<long> GetAsync(CancellationToken ct);
    
    Task<long> AddUnitsAsync(long units, CancellationToken ct);
}