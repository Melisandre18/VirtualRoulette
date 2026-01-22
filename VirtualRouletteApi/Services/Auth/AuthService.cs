using Microsoft.AspNetCore.Identity;
using VirtualRouletteApi.Domain;
using VirtualRouletteApi.Dtos;
using VirtualRouletteApi.Infrastructure.Storage;

namespace VirtualRouletteApi.Services.Auth;

public class AuthService(IUserStore users, IPasswordHasher<User> passwordHasher) : IAuthService
{
    public async Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (!IsValid(req.UserName, req.Password))
            return AuthResult<RegisterResponse>.Fail(AuthError.InvalidInput, "Username and password are required.");

        var normalizedUserName = req.UserName.Trim();

        var exists = await users.UserNameExistsAsync(normalizedUserName, ct);
        if (exists)
            return AuthResult<RegisterResponse>.Fail(AuthError.UsernameTaken, "Username already exists.");

        var user = new User
        {
            UserName = normalizedUserName,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, req.Password);

        await users.AddAsync(user, ct);
        await users.SaveAsync(ct);

        return AuthResult<RegisterResponse>.Ok(new RegisterResponse(user.Id, user.UserName));
    }

    public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        if (!IsValid(req.UserName, req.Password))
            return AuthResult<LoginResponse>.Fail(AuthError.InvalidInput, "Username and password are required.");

        var normalizedUserName = req.UserName.Trim();

        var user = await users.FindByUserNameAsync(normalizedUserName, ct);
        if (user is null)
            return AuthResult<LoginResponse>.Fail(AuthError.InvalidCredentials, "Invalid username or password.");

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed)
            return AuthResult<LoginResponse>.Fail(AuthError.InvalidCredentials, "Invalid username or password.");
        
        user.IsActive = true;
        user.LastSeen = DateTimeOffset.UtcNow;
        await users.SaveAsync(ct);
        
        return AuthResult<LoginResponse>.Ok(new LoginResponse("Valid credentials (Basic mode)."));
    }
    
    public async Task LogoutAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId, ct);
        if (user is null) return;

        user.IsActive = false;
        await users.SaveAsync(ct);
    }


    private static bool IsValid(string userName, string password)
        => !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password);
}
