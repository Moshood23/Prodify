namespace Prodify.Application.Common.Interfaces;

public record IdentityResult(
    bool Succeeded,
    Guid? UserId,
    string? Token,
    IEnumerable<string> Errors,
    string? RefreshToken = null);

public interface IIdentityService
{
    Task<IdentityResult> RegisterAsync(string email, string password, Guid? customerId, Guid? sellerId, string role, CancellationToken cancellationToken = default);
    Task<IdentityResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    // Links a seller profile to an existing user, grants the Seller role and returns fresh tokens.
    Task<IdentityResult> AddSellerAccountAsync(Guid userId, Guid sellerId, CancellationToken cancellationToken = default);

    // Exchanges a valid refresh token for a new access token + refresh token (the old one is revoked).
    Task<IdentityResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    // Changes the password, logs the user out everywhere else and returns fresh tokens for this session.
    Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    // A one-time password reset token (URL-safe) for the account with this email,
    // or null if there is no such account. Valid for 1 hour.
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);

    // Sets a new password using a token from GeneratePasswordResetTokenAsync and
    // logs the user out everywhere. No tokens are returned: the user logs in again.
    Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

    // Revokes a refresh token that belongs to the given user (logout).
    Task RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
}