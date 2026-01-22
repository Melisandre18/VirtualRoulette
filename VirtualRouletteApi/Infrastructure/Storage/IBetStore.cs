using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage;

public interface IBetStore
{
    Task AddAsync(Bet bet, CancellationToken ct);

    Task<IReadOnlyList<Bet>> GetHistoryAsync(Guid userId, int take, CancellationToken ct);
}