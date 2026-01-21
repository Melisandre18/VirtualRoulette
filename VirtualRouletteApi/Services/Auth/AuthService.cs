using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Domain;
using VirtualRouletteApi.Dtos;

namespace VirtualRouletteApi.Services.Auth;

public class AuthService(AppDbContext db, IPasswordHasher<User> passwordHasher) : IAuthService
{
    public async Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (!IsValid(req.UserName, req.Password))
            return AuthResult<RegisterResponse>.Fail(AuthError.InvalidInput, "Username and password are required.");

        var normalizedUserName = req.UserName.Trim();

        var exists = await db.Users.AnyAsync(u => u.UserName == normalizedUserName, ct);
        if (exists)
            return AuthResult<RegisterResponse>.Fail(AuthError.UsernameTaken, "Username already exists.");

        var user = new User
        {
            UserName = normalizedUserName,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, req.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return AuthResult<RegisterResponse>.Ok(new RegisterResponse(user.Id, user.UserName));
    }

    public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        if (!IsValid(req.UserName, req.Password))
            return AuthResult<LoginResponse>.Fail(AuthError.InvalidInput, "Username and password are required.");

        var normalizedUserName = req.UserName.Trim();

        var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == normalizedUserName, ct);
        if (user is null)
            return AuthResult<LoginResponse>.Fail(AuthError.InvalidCredentials, "Invalid username or password.");

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed)
            return AuthResult<LoginResponse>.Fail(AuthError.InvalidCredentials, "Invalid username or password.");
        
        user.IsActive = true;
        user.LastSeen = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        
        return AuthResult<LoginResponse>.Ok(new LoginResponse("Valid credentials (Basic mode)."));
    }
    
    public async Task LogoutAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;

        user.IsActive = false;
        await db.SaveChangesAsync(ct);
    }


    private static bool IsValid(string userName, string password)
        => !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password);
}
