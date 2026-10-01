using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Infrastructure.Persistence.Seed;

// Development only: a few sample reviews on the demo store's products, so the
// shop shows star ratings straight away. Runs once (skips if any exist).
public class DemoReviewSeeder
{
    private static readonly string[] Reviewers =
    {
        "Chioma O.", "Tunde A.", "Aisha B.", "Emeka N.", "Funke L.", "Ibrahim S.", "Ngozi E.", "Segun F.",
    };

    // (rating, title, comment); each product gets a few of these, in turn.
    private static readonly (int Rating, string Title, string Comment)[] Samples =
    {
        (5, "Exactly as described", "Arrived in two days, well packed and working perfectly. Very happy with it."),
        (4, "Good value", "Does the job well for the price. Delivery took a little longer than expected."),
        (5, "Highly recommend", "Great quality. I have already told my friends to buy from this store."),
        (3, "Okay", "It's fine, but not as good as I hoped. The seller replied quickly to my questions."),
        (5, "Love it", "Better than the one I had before. Paid on delivery and the rider was polite."),
        (4, "Nice", "Good product. The box was a bit dented but everything inside was okay."),
    };

    private readonly ProdifyDbContext _context;
    private readonly DemoDataSettings _settings;
    private readonly ILogger<DemoReviewSeeder> _logger;

    public DemoReviewSeeder(ProdifyDbContext context, IOptions<DemoDataSettings> settings, ILogger<DemoReviewSeeder> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (!_settings.Enabled || await _context.ProductReviews.AnyAsync())
            return;

        var sellerEmail = _settings.SellerEmail.Trim().ToLowerInvariant();
        var productIds = await _context.Products
            .Where(p => _context.Sellers.Any(s => s.Id == p.SellerId && s.Email == sellerEmail))
            .OrderBy(p => p.Name)
            .Select(p => p.Id)
            .ToListAsync();

        var sample = 0;
        for (var i = 0; i < productIds.Count; i++)
        {
            // Some products get no reviews, like in a real shop.
            if (i % 4 == 3)
                continue;

            var count = 2 + i % 3;
            for (var n = 0; n < count; n++)
            {
                var (rating, title, comment) = Samples[sample++ % Samples.Length];
                var reviewer = Reviewers[(i + n) % Reviewers.Length];
                _context.ProductReviews.Add(ProductReview.Create(productIds[i], Guid.NewGuid(), reviewer, rating, title, comment));
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Added demo reviews to {Count} products", productIds.Count - productIds.Count / 4);
    }
}
