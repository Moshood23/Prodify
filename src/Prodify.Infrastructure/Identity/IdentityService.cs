using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Infrastructure.Persistence;

namespace Prodify.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly ProdifyDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        JwtService jwtService,
        ProdifyDbContext dbContext,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Application.Common.Interfaces.IdentityResult> RegisterAsync(
        string email, string password, Guid? customerId, Guid? sellerId, string role, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            CustomerId = customerId,
            SellerId = sellerId
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return Failure(result.Errors.Select(e => e.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
            return Failure(roleResult.Errors.Select(e => e.Description));

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Application.Common.Interfaces.IdentityResult> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            return Failure("Invalid email or password.");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Application.Common.Interfaces.IdentityResult> AddSellerAccountAsync(
        Guid userId, Guid sellerId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Failure("User not found.");

        if (user.SellerId.HasValue)
            return Failure("This account is already registered as a seller.");

        user.SellerId = sellerId;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return Failure(updateResult.Errors.Select(e => e.Description));

        if (!await _userManager.IsInRoleAsync(user, Roles.Seller))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Seller);

            if (!roleResult.Succeeded)
                return Failure(roleResult.Errors.Select(e => e.Description));
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Application.Common.Interfaces.IdentityResult> RefreshAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = Hash(refreshToken);

        var stored = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (stored is null)
            return Failure("Invalid refresh token.");

        if (stored.RevokedAt is not null)
        {
            if (stored.ReplacedByTokenHash is not null)
                await RevokeAllForUserAsync(stored.UserId, cancellationToken);

            return Failure("Invalid refresh token.");
        }

        if (!stored.IsActive)
            return Failure("Refresh token has expired.");

        var user = await _userManager.FindByIdAsync(stored.UserId.ToString());

        if (user is null)
            return Failure("Invalid refresh token.");

        // Rotation: every refresh token can be used once.
        var (newToken, newHash) = GenerateRefreshToken();
        stored.Revoke(replacedByTokenHash: newHash);

        return await IssueTokensAsync(user, cancellationToken, newToken, newHash);
    }

    public async Task<Application.Common.Interfaces.IdentityResult> ChangePasswordAsync(
    Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Failure("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
            return Failure(result.Errors.Select(e => e.Code == "PasswordMismatch" ? "Your current password is incorrect." : e.Description));

        // Someone who knew the old password may still be logged in elsewhere: end those sessions.
        await RevokeAllForUserAsync(user.Id, cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }


    public async Task RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = Hash(refreshToken);

        var stored = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UserId == userId, cancellationToken);

        // Logging out with an unknown or already revoked token is not an error.
        if (stored is null || stored.RevokedAt is not null)
            return;

        stored.Revoke();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Application.Common.Interfaces.IdentityResult> IssueTokensAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        string? refreshToken = null,
        string? refreshTokenHash = null)
    {
        if (refreshToken is null || refreshTokenHash is null)
            (refreshToken, refreshTokenHash) = GenerateRefreshToken();

        _dbContext.RefreshTokens.Add(RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)));

        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateToken(user, roles);

        return new Application.Common.Interfaces.IdentityResult(
            true, user.Id, accessToken, Enumerable.Empty<string>(), refreshToken);
    }

    private async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static (string Token, string Hash) GenerateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (token, Hash(token));
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static Application.Common.Interfaces.IdentityResult Failure(params string[] errors) =>
        Failure((IEnumerable<string>)errors);

    private static Application.Common.Interfaces.IdentityResult Failure(IEnumerable<string> errors) =>
        new(false, null, null, errors);
}