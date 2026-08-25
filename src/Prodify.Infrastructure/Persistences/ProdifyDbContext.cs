using Microsoft.EntityFrameworkCore;
using Prodify.Domain.Cart.Entities;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Customers.Entities;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Payments.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Infrastructure.Persistence;

public class ProdifyDbContext : DbContext
{
    public ProdifyDbContext(DbContextOptions<ProdifyDbContext> options) : base(options)
    {
    }

    // Catalog
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();

    // Inventory
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    // Cart
    public DbSet<Domain.Cart.Entities.Cart> Carts => Set<Domain.Cart.Entities.Cart>();

    // Ordering
    public DbSet<Order> Orders => Set<Order>();

    // Payments
    public DbSet<Payment> Payments => Set<Payment>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();

    // Sellers
    public DbSet<Seller> Sellers => Set<Seller>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdifyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}