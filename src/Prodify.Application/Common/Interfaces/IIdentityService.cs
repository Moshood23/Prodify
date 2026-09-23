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

    // Revokes a refresh token that belongs to the given user (logout).
    Task RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
}