namespace VirtualRouletteApi.Dtos;

public record BetHistory(
    Guid BetId,
    long BetAmount,
    long WinAmount,
    DateTimeOffset CreatedAt
);
