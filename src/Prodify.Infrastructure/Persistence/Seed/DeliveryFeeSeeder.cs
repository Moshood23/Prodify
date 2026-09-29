using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Infrastructure.Persistence.Seed;

// Makes sure every state has a delivery fee. Only adds missing states, so fees
// an admin has changed are never overwritten. Runs on every start-up.
public class DeliveryFeeSeeder
{
    // Starting fees in naira; admins can change them under Admin → Delivery fees.
    // The state names match the checkout address form.
    private static readonly Dictionary<string, decimal> DefaultFees = new()
    {
        ["Lagos"] = 2500,

        ["Ogun"] = 3500, ["Oyo"] = 3500, ["Osun"] = 3500, ["Ondo"] = 3500, ["Ekiti"] = 3500,
        ["FCT - Abuja"] = 3500,

        ["Edo"] = 4000, ["Delta"] = 4000, ["Rivers"] = 4000, ["Bayelsa"] = 4000, ["Akwa Ibom"] = 4000,
        ["Cross River"] = 4000, ["Anambra"] = 4000, ["Enugu"] = 4000, ["Imo"] = 4000, ["Abia"] = 4000,
        ["Ebonyi"] = 4000, ["Kwara"] = 4000, ["Kogi"] = 4000, ["Niger"] = 4000, ["Nasarawa"] = 4000,
        ["Plateau"] = 4000, ["Benue"] = 4000,

        ["Kaduna"] = 5000, ["Kano"] = 5000, ["Katsina"] = 5000, ["Jigawa"] = 5000, ["Kebbi"] = 5000,
        ["Sokoto"] = 5000, ["Zamfara"] = 5000, ["Bauchi"] = 5000, ["Gombe"] = 5000, ["Adamawa"] = 5000,
        ["Taraba"] = 5000, ["Borno"] = 5000, ["Yobe"] = 5000,
    };

    private readonly ProdifyDbContext _context;
    private readonly ILogger<DeliveryFeeSeeder> _logger;

    public DeliveryFeeSeeder(ProdifyDbContext context, ILogger<DeliveryFeeSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var existing = await _context.DeliveryFees.Select(f => f.State).ToListAsync();
        var missing = DefaultFees.Keys.Except(existing, StringComparer.OrdinalIgnoreCase).ToList();

        if (missing.Count == 0)
            return;

        foreach (var state in missing)
            _context.DeliveryFees.Add(DeliveryFee.Create(state, DefaultFees[state]));

        await _context.SaveChangesAsync();
        _logger.LogInformation("Added delivery fees for {Count} states", missing.Count);
    }
}
