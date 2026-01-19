namespace VirtualRouletteApi.Dtos;

public record RegisterResponse(Guid Id, string UserName);
public record LoginResponse(string Message);