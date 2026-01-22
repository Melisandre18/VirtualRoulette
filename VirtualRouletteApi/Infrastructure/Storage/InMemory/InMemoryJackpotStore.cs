using System.Threading;

namespace VirtualRouletteApi.Infrastructure.Storage.InMemory;

public sealed class InMemoryJackpotStore(InMemoryState state) : IJackpotStore
{
    public Task<long> GetAsync(CancellationToken ct) =>
        Task.FromResult(Interlocked.Read(ref state.JackpotUnits));

    public Task<long> AddUnitsAsync(long units, CancellationToken ct)
    {
        var next = Interlocked.Add(ref state.JackpotUnits, units);
        return Task.FromResult(next);
    }
}