using System.Security.Cryptography;
using ge.singular.roulette;

namespace VirtualRouletteApi.Services.Roulette;

public class RouletteService : IRouletteService
{
    public bool ValidateBet(string betJson, out long betAmount)
    {
        betAmount = 0;

        var result = CheckBets.IsValid(betJson);
        if (!result.getIsValid())
            return false;

        betAmount = result.getBetAmount();
        return true;
    }

    public int GenerateWinningNumber()
    {
        return RandomNumberGenerator.GetInt32(0, 37);
    }

    public long CalculateWin(string betJson, int winningNumber)
    {
        return CheckBets.EstimateWin(betJson, winningNumber);
    }
}