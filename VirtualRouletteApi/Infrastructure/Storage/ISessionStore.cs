namespace VirtualRouletteApi.Infrastructure.Storage;

public interface ISessionStore
{
    Task<int> SignOutInactiveAsync(DateTimeOffset cutoff, CancellationToken ct);
}