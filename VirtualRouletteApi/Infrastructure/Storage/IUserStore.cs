using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Infrastructure.Storage;

public interface IUserStore
{
    Task<bool> UserNameExistsAsync(string userName, CancellationToken ct);
    
    Task<User?> FindByUserNameAsync(string userName, CancellationToken ct);
    
    Task<User?> FindByIdAsync(Guid userId, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);
    
    Task SaveAsync(CancellationToken ct);
}