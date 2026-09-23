using Microsoft.AspNetCore.Identity;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;

    public IdentityService(UserManager<ApplicationUser> userManager, JwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
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
        {
            return new Application.Common.Interfaces.IdentityResult(
                false, null, null, result.Errors.Select(e => e.Description));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            return new Application.Common.Interfaces.IdentityResult(
                false, null, null, roleResult.Errors.Select(e => e.Description));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return new Application.Common.Interfaces.IdentityResult(true, user.Id, token, Enumerable.Empty<string>());
    }

    public async Task<Application.Common.Interfaces.IdentityResult> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return new Application.Common.Interfaces.IdentityResult(
                false, null, null, new[] { "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return new Application.Common.Interfaces.IdentityResult(true, user.Id, token, Enumerable.Empty<string>());
    }
}