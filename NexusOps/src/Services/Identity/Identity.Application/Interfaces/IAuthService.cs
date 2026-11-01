
using Identity.Application.DTOs;

namespace Identity.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<AuthResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    Task<AuthResult> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken);

    Task<AuthResult> LogoutAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken);
}
