
namespace Identity.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public Guid Version { get; private set; } = Guid.NewGuid();

    private RefreshToken() { }

    public RefreshToken(
        string userId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        Version = Guid.NewGuid();
    }

    public bool IsActive =>
        RevokedAtUtc is null &&
        ExpiresAtUtc > DateTime.UtcNow;

    public void Revoke(string? replacementHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            throw new InvalidOperationException(
                "Refresh token has already been revoked.");
        }

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacementHash;
        Version = Guid.NewGuid();
    }
}
