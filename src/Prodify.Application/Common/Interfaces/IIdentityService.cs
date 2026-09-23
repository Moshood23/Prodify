namespace Prodify.Application.Common.Interfaces;

public record IdentityResult(bool Succeeded, Guid? UserId, string? Token, IEnumerable<string> Errors);

public interface IIdentityService
{
    Task<IdentityResult> RegisterAsync(string email, string password, Guid? customerId, Guid? sellerId, string role, CancellationToken cancellationToken = default);
    Task<IdentityResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    // Links a seller profile to an existing user, grants the Seller role and returns a fresh token.
    Task<IdentityResult> AddSellerAccountAsync(Guid userId, Guid sellerId, CancellationToken cancellationToken = default);
}