using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage.InMemory;

public sealed class InMemoryUserStore(InMemoryState state) : IUserStore
{
    public Task<bool> UserNameExistsAsync(string userName, CancellationToken ct) =>
        Task.FromResult(state.UserIdsByName.ContainsKey(userName));

    public Task<User?> FindByUserNameAsync(string userName, CancellationToken ct)
    {
        if (!state.UserIdsByName.TryGetValue(userName, out var id))
            return Task.FromResult<User?>(null);

        state.UsersById.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct)
    {
        state.UsersById.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task AddAsync(User user, CancellationToken ct)
    {
        state.UsersById[user.Id] = user;
        state.UserIdsByName[user.UserName] = user.Id;
        return Task.CompletedTask;
    }

    public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
}