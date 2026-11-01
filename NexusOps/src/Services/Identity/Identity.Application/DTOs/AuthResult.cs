
namespace Identity.Application.DTOs;

public sealed record AuthResult(
    bool Succeeded,
    string Message,
    AuthResponse? Data = null)
{
    public static AuthResult Success(
        AuthResponse data,
        string message = "Operation successful.")
        => new(true, message, data);

    public static AuthResult Failure(string message)
        => new(false, message);
}
