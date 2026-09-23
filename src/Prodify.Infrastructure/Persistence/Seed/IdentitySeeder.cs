using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prodify.Application.Common.Security;
using Prodify.Infrastructure.Identity;

namespace Prodify.Infrastructure.Persistence.Seed;

public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedAdminSettings _adminSettings;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SeedAdminSettings> adminSettings,
        ILogger<IdentitySeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _adminSettings = adminSettings.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await BackfillExistingUserRolesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (await _roleManager.RoleExistsAsync(role))
                continue;

            await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            _logger.LogInformation("Created role {Role}", role);
        }
    }

    // Users created before roles existed have a CustomerId/SellerId but no role.
    private async Task BackfillExistingUserRolesAsync()
    {
        var users = await _userManager.Users
            .Where(u => u.CustomerId != null || u.SellerId != null)
            .ToListAsync();

        foreach (var user in users)
        {
            if (user.CustomerId.HasValue && !await _userManager.IsInRoleAsync(user, Roles.Customer))
                await _userManager.AddToRoleAsync(user, Roles.Customer);

            if (user.SellerId.HasValue && !await _userManager.IsInRoleAsync(user, Roles.Seller))
                await _userManager.AddToRoleAsync(user, Roles.Seller);
        }
    }

    private async Task SeedAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(_adminSettings.Email) || string.IsNullOrWhiteSpace(_adminSettings.Password))
        {
            _logger.LogInformation("SeedAdmin settings not provided; skipping admin seeding.");
            return;
        }

        var admin = await _userManager.FindByEmailAsync(_adminSettings.Email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = _adminSettings.Email,
                Email = _adminSettings.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, _adminSettings.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(" ", result.Errors.Select(e => e.Description)));
                return;
            }

            _logger.LogInformation("Created admin user {Email}", _adminSettings.Email);
        }

        if (!await _userManager.IsInRoleAsync(admin, Roles.Admin))
            await _userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}