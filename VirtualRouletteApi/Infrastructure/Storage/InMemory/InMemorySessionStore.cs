namespace VirtualRouletteApi.Infrastructure.Storage.InMemory;

public sealed class InMemorySessionStore(InMemoryState state) : ISessionStore
{
    public Task<int> SignOutInactiveAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        var count = 0;

        foreach (var kv in state.UsersById)
        {
            var user = kv.Value;

            lock (user)
            {
                if (user.IsActive && user.LastSeen < cutoff)
                {
                    user.IsActive = false;
                    count++;
                }
            }
        }

        return Task.FromResult(count);
    }
}