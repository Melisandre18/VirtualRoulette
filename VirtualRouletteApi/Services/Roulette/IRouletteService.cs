namespace VirtualRouletteApi.Services.Roulette;

public interface IRouletteService
{
    bool ValidateBet(string betJson, out long betAmount);
    
    int GenerateWinningNumber();
    
    long CalculateWin(string betJson, int winningNumber);
}