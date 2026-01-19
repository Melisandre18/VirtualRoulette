namespace VirtualRouletteApi.Domain;

public class Bet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    
    public required string BetJson { get; set; }

    public long BetAmount { get; set; }

    public int WinningNumber { get; set; }

    public long WinAmount { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}