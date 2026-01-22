using System.Collections.Concurrent;
using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage.InMemory;

public sealed class InMemoryState
{
    public ConcurrentDictionary<Guid, User> UsersById { get; } = new();

    public ConcurrentDictionary<string, Guid> UserIdsByName { get; } = new(StringComparer.Ordinal);

    public ConcurrentDictionary<Guid, ConcurrentQueue<Bet>> BetsByUser { get; } = new();

    public long JackpotUnits;
}
    