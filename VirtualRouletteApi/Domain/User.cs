namespace VirtualRouletteApi.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserName { get; set; }
    public required string PasswordHash { get; set; }
    public long Balance { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;

    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
