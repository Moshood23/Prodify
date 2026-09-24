using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prodify.Application.Common.Security;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Sellers.Entities;
using Prodify.Infrastructure.Identity;

namespace Prodify.Infrastructure.Persistence.Seed;

// Development-only sample catalog so the storefront has something to show:
// categories, brands, an approved demo seller (who can log in), products with
// variants and images, and stock in a Lagos warehouse.
// Runs once: if the demo seller already exists, nothing is seeded.
public class DemoCatalogSeeder
{
    private readonly ProdifyDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DemoDataSettings _settings;
    private readonly ILogger<DemoCatalogSeeder> _logger;

    public DemoCatalogSeeder(
        ProdifyDbContext context,
        UserManager<ApplicationUser> userManager,
        IOptions<DemoDataSettings> settings,
        ILogger<DemoCatalogSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (!_settings.Enabled)
            return;

        var sellerEmail = _settings.SellerEmail.Trim().ToLowerInvariant();

        if (await _context.Sellers.AnyAsync(s => s.Email == sellerEmail))
            return;

        var seller = Seller.Create("Prodify Demo Store", sellerEmail, "+234 800 000 0000");
        seller.Approve();
        _context.Sellers.Add(seller);

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == "LOS-MAIN");
        if (warehouse is null)
        {
            warehouse = Warehouse.Create("Lagos Main Warehouse", "LOS-MAIN", "Ikeja, Lagos");
            _context.Warehouses.Add(warehouse);
        }

        var categories = new Dictionary<string, Category>();
        var brands = new Dictionary<string, Brand>();

        foreach (var item in DemoProducts)
        {
            var category = await GetOrCreateCategoryAsync(item.Category, categories);
            var brand = item.Brand is null ? null : await GetOrCreateBrandAsync(item.Brand, brands);

            var product = Product.Create(item.Name, item.Description, category.Id, seller.Id, brand?.Id);

            product.AddImage(ImageUrl(item.Name, "e6f4f1", "0e7c66"), item.Name);
            product.AddImage(ImageUrl(item.Name, "fef3c7", "92400e"), item.Name);

            foreach (var (name, value) in item.Attributes)
                product.AddAttribute(name, value);

            _context.Products.Add(product);

            foreach (var variant in item.Variants)
            {
                var productVariant = ProductVariant.Create(
                    product.Id, variant.Sku, variant.Price, variant.Name, variant.CompareAtPrice, variant.Weight);
                _context.ProductVariants.Add(productVariant);

                _context.InventoryItems.Add(InventoryItem.Create(productVariant.Id, warehouse.Id, variant.Stock));
            }
        }

        await _context.SaveChangesAsync();

        await CreateSellerLoginAsync(seller.Id, sellerEmail);

        _logger.LogInformation("Seeded demo catalog: {Count} products for {Seller}", DemoProducts.Length, sellerEmail);
    }

    // So the seller portal can be tried out with real data.
    private async Task CreateSellerLoginAsync(Guid sellerId, string email)
    {
        if (string.IsNullOrWhiteSpace(_settings.SellerPassword))
            return;

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, SellerId = sellerId };
            var result = await _userManager.CreateAsync(user, _settings.SellerPassword);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create demo seller user: {Errors}",
                    string.Join(" ", result.Errors.Select(e => e.Description)));
                return;
            }
        }
        else if (user.SellerId is null)
        {
            user.SellerId = sellerId;
            await _userManager.UpdateAsync(user);
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Seller))
            await _userManager.AddToRoleAsync(user, Roles.Seller);
    }

    private async Task<Category> GetOrCreateCategoryAsync(string name, Dictionary<string, Category> cache)
    {
        if (cache.TryGetValue(name, out var cached))
            return cached;

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name)
            ?? _context.Categories.Add(Category.Create(name)).Entity;

        cache[name] = category;
        return category;
    }

    private async Task<Brand> GetOrCreateBrandAsync(string name, Dictionary<string, Brand> cache)
    {
        if (cache.TryGetValue(name, out var cached))
            return cached;

        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Name == name)
            ?? _context.Brands.Add(Brand.Create(name)).Entity;

        cache[name] = brand;
        return brand;
    }

    // Placeholder images in the Prodify colours; sellers replace them with real photos.
    private static string ImageUrl(string text, string background, string foreground) =>
        $"https://placehold.co/600x600/{background}/{foreground}/png?text={Uri.EscapeDataString(text).Replace("%20", "+")}";

    private sealed record DemoVariant(string Sku, decimal Price, int Stock, string? Name = null, decimal? CompareAtPrice = null, decimal Weight = 0.5m);

    private sealed record DemoProduct(
        string Name,
        string Category,
        string? Brand,
        string Description,
        (string Name, string Value)[] Attributes,
        DemoVariant[] Variants);

    private static readonly DemoProduct[] DemoProducts =
    {
        new("Tecno Spark 20 Pro", "Phones & Tablets", "Tecno",
            "6.78\" FHD+ display, 108MP camera and a 5000mAh battery that lasts all day.",
            new[] { ("Display", "6.78 inches"), ("Battery", "5000mAh"), ("Camera", "108MP") },
            new[]
            {
                new DemoVariant("TEC-SPK20P-128", 189_000m, 25, "8GB RAM / 128GB", 215_000m, 0.4m),
                new DemoVariant("TEC-SPK20P-256", 219_000m, 12, "8GB RAM / 256GB", 245_000m, 0.4m)
            }),
        new("Samsung Galaxy A15", "Phones & Tablets", "Samsung",
            "Super AMOLED display, 50MP triple camera and 25W fast charging.",
            new[] { ("Display", "6.5 inches"), ("Battery", "5000mAh") },
            new[] { new DemoVariant("SAM-A15-128", 245_000m, 18, "6GB RAM / 128GB", 270_000m, 0.4m) }),
        new("Infinix Hot 40i", "Phones & Tablets", "Infinix",
            "Big screen, big battery, great value.",
            new[] { ("Display", "6.56 inches"), ("Battery", "5000mAh") },
            new[] { new DemoVariant("INF-HOT40I-128", 135_000m, 30, "4GB RAM / 128GB", null, 0.4m) }),
        new("HP 250 G9 Laptop", "Computing", "HP",
            "Intel Core i5, 8GB RAM, 512GB SSD, 15.6\" FHD screen. Ideal for work and school.",
            new[] { ("Processor", "Intel Core i5-1235U"), ("RAM", "8GB"), ("Storage", "512GB SSD") },
            new[] { new DemoVariant("HP-250G9-I5", 650_000m, 8, null, 720_000m, 1.8m) }),
        new("Oraimo FreePods 4", "Electronics", "Oraimo",
            "Active noise cancellation with up to 35 hours of playtime.",
            new[] { ("Battery life", "35 hours"), ("Noise cancellation", "Yes") },
            new[] { new DemoVariant("ORA-FP4-BLK", 28_500m, 60, "Black", 35_000m, 0.1m) }),
        new("Hisense 43\" Smart TV", "Electronics", "Hisense",
            "Full HD smart TV with Netflix, YouTube and Bluetooth.",
            new[] { ("Screen size", "43 inches"), ("Resolution", "1920 x 1080") },
            new[] { new DemoVariant("HIS-43A4-FHD", 310_000m, 0, null, 340_000m, 8m) }),
        new("Men's Classic Ankara Shirt", "Fashion", null,
            "100% cotton Ankara print shirt, tailored in Lagos.",
            new[] { ("Material", "Cotton"), ("Fit", "Regular") },
            new[]
            {
                new DemoVariant("ANK-SHIRT-M", 15_000m, 20, "Medium", null, 0.3m),
                new DemoVariant("ANK-SHIRT-L", 15_000m, 15, "Large", null, 0.3m),
                new DemoVariant("ANK-SHIRT-XL", 16_500m, 6, "X-Large", null, 0.3m)
            }),
        new("Women's Running Sneakers", "Fashion", "Adidas",
            "Lightweight, breathable sneakers for running and everyday wear.",
            new[] { ("Upper", "Mesh"), ("Sole", "Rubber") },
            new[]
            {
                new DemoVariant("ADI-RUN-W-38", 42_000m, 10, "Size 38", 55_000m, 0.8m),
                new DemoVariant("ADI-RUN-W-40", 42_000m, 7, "Size 40", 55_000m, 0.8m)
            }),
        new("Binatone 1.7L Electric Kettle", "Home & Kitchen", "Binatone",
            "Stainless steel kettle with auto shut-off.",
            new[] { ("Capacity", "1.7 litres"), ("Power", "2200W") },
            new[] { new DemoVariant("BIN-KET-17", 18_500m, 40, null, 22_000m, 1.2m) }),
        new("Scanfrost 5-Burner Gas Cooker", "Home & Kitchen", "Scanfrost",
            "Gas cooker with oven and grill, auto ignition.",
            new[] { ("Burners", "5"), ("Oven", "Yes") },
            new[] { new DemoVariant("SCF-GC-5B", 385_000m, 4, null, null, 45m) }),
        new("Nivea Men Deep Clean Face Wash", "Health & Beauty", "Nivea",
            "Deep cleansing face wash with active charcoal.",
            new[] { ("Size", "100ml") },
            new[] { new DemoVariant("NIV-FW-100", 4_200m, 100, null, 5_000m, 0.15m) }),
        new("Mamador Pure Vegetable Oil 3L", "Supermarket", "Mamador",
            "Cholesterol-free vegetable oil for everyday cooking.",
            new[] { ("Volume", "3 litres") },
            new[] { new DemoVariant("MAM-VO-3L", 11_800m, 80, null, null, 3m) }),
        new("Golden Penny Spaghetti (Pack of 20)", "Supermarket", "Golden Penny",
            "20 x 500g packs of spaghetti.",
            new[] { ("Pack size", "20 x 500g") },
            new[] { new DemoVariant("GP-SPAG-20", 19_500m, 50, null, 21_000m, 10m) }),
        new("PlayStation 5 DualSense Controller", "Gaming", "Sony",
            "Haptic feedback and adaptive triggers for the PS5.",
            new[] { ("Compatibility", "PlayStation 5"), ("Connection", "Bluetooth / USB-C") },
            new[] { new DemoVariant("SONY-DS-WHT", 95_000m, 14, "White", 110_000m, 0.3m) })
    };
}