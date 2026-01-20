namespace VirtualRouletteApi.Dtos;

public record BetRequest(string Bet);

public record BetResponse(
    string Status,
    Guid BetId,
    int WinningNumber,
    long WinAmount
);
