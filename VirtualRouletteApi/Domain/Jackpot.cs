namespace VirtualRouletteApi.Domain;

public class Jackpot
{
    public int Id { get; set; } = 1;
    
    public long Amount { get; set; } = 0;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}