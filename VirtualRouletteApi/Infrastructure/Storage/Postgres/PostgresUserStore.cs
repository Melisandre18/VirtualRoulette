using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage.Postgres;

public sealed class PostgresUserStore(AppDbContext db) : IUserStore
{
    public Task<bool> UserNameExistsAsync(string userName, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.UserName == userName, ct);

    public Task<User?> FindByUserNameAsync(string userName, CancellationToken ct) =>
        db.Users.SingleOrDefaultAsync(u => u.UserName == userName, ct);

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct) =>
        db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);

    public Task AddAsync(User user, CancellationToken ct)
    {
        db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}