namespace VirtualRouletteApi.Infrastructure.Storage;

public interface IBalanceStore
{
    Task<long> GetAsync(Guid userId, CancellationToken ct);
    
    Task<long> AddAsync(Guid userId, long amount, CancellationToken ct);
    
    Task<long> SubtractAsync(Guid userId, long amount, CancellationToken ct);
}