
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Identity;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IdentityDbContext dbContext,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return AuthResult.Failure(
                "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(x => x.Description));

            return AuthResult.Failure(errors);
        }

        // Public registration always creates an Employee account.
        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Employee");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(x => x.Description));

            return AuthResult.Failure(
                $"Role assignment failed: {errors}");
        }

        var response = await CreateAuthResponseAsync(
            user,
            cancellationToken);

        return AuthResult.Success(response);
    }

    // =========================================================
    // LOGIN
    // =========================================================

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthResult.Failure(
                "Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return AuthResult.Failure(
                "Your account is temporarily locked. Please try again later.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);

            return AuthResult.Failure(
                "Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var response = await CreateAuthResponseAsync(
            user,
            cancellationToken);

        return AuthResult.Success(response);
    }

    // =========================================================
    // REFRESH TOKEN
    // =========================================================

    public async Task<AuthResult> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return AuthResult.Failure(
                "Refresh token is required.");
        }

        // Hash the incoming token.
        var tokenHash = JwtTokenService.HashRefreshToken(
            request.RefreshToken);

        // Find the matching database record.
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return AuthResult.Failure(
                "Invalid, expired, or already used refresh token.");
        }

        var user = await _userManager.FindByIdAsync(
            storedToken.UserId);

        if (user is null)
        {
            return AuthResult.Failure(
                "User account could not be found.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            // IMPORTANT:
            // GenerateRefreshToken returns a tuple.
            // Deconstruct it into two separate variables.

            var (newRefreshToken, newRefreshTokenExpiresAtUtc) =
                _jwtTokenService.GenerateRefreshToken();

            // Hash the new refresh token.
            var newRefreshTokenHash =
                JwtTokenService.HashRefreshToken(newRefreshToken);

            // Revoke the old refresh token.
            storedToken.Revoke(newRefreshTokenHash);

            // Create the replacement refresh-token record.
            var replacementToken = new RefreshToken(
                user.Id,
                newRefreshTokenHash,
                newRefreshTokenExpiresAtUtc);

            await _dbContext.RefreshTokens.AddAsync(
                replacementToken,
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Generate a new access token.
            var roles = await _userManager.GetRolesAsync(user);

            var (accessToken, accessTokenExpiresAtUtc) =
                _jwtTokenService.GenerateAccessToken(user, roles);

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToArray()
            };

            return AuthResult.Success(response);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);

            return AuthResult.Failure(
                "This refresh token has already been used. Please log in again.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    public async Task<AuthResult> LogoutAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return AuthResult.Failure(
                "Refresh token is required.");
        }

        var tokenHash = JwtTokenService.HashRefreshToken(
            request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null)
        {
            return AuthResult.Failure(
                "Refresh token was not found.");
        }

        if (!storedToken.IsActive)
        {
            return AuthResult.Failure(
                "Refresh token has already expired or been revoked.");
        }

        storedToken.Revoke();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(
            new AuthResponse
            {
                UserId = storedToken.UserId
            });
    }

    // =========================================================
    // CREATE AUTH RESPONSE
    // =========================================================

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);

        // Generate access token.
        var (accessToken, accessTokenExpiresAtUtc) =
            _jwtTokenService.GenerateAccessToken(user, roles);

        // Generate refresh token and expiry.
        // Deconstruct the tuple directly.
        var (refreshToken, refreshTokenExpiresAtUtc) =
            _jwtTokenService.GenerateRefreshToken();

        // Store only the SHA-256 hash in the database.
        var refreshTokenHash =
            JwtTokenService.HashRefreshToken(refreshToken);

        var refreshTokenEntity = new RefreshToken(
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAtUtc);

        await _dbContext.RefreshTokens.AddAsync(
            refreshTokenEntity,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToArray()
        };
    }
}
