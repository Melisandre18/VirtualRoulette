namespace VirtualRouletteApi.Services.Auth;

public enum AuthError
{
    InvalidInput,
    UsernameTaken,
    InvalidCredentials
}

public record AuthResult<T>(T? Value, AuthError? Error, string? Message)
{
    public bool Success => Error is null;

    public static AuthResult<T> Ok(T value)
        => new(value, null, null);

    public static AuthResult<T> Fail(AuthError error, string? message = null)
        => new(default, error, message);
}