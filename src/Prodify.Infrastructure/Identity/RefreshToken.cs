namespace Prodify.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    // Only a SHA-256 hash is stored, so a database leak does not expose usable tokens.
    public string TokenHash { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken()
    {
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = tokenHash,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = expiresAt
    };

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt ??= DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}