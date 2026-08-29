using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Cart.Entities;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Customers.Entities;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Payments.Entities;
using Prodify.Domain.Sellers.Entities;
using Prodify.Infrastructure.Identity;
using Prodify.Infrastructure.Messaging.Outbox;

namespace Prodify.Infrastructure.Persistence;

public class ProdifyDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
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

    // Messaging
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // IApplicationDbContext explicit IQueryable projections
    IQueryable<Product> IApplicationDbContext.Products => Products;
    IQueryable<ProductVariant> IApplicationDbContext.ProductVariants => ProductVariants;
    IQueryable<Category> IApplicationDbContext.Categories => Categories;
    IQueryable<Brand> IApplicationDbContext.Brands => Brands;
    IQueryable<Warehouse> IApplicationDbContext.Warehouses => Warehouses;
    IQueryable<InventoryItem> IApplicationDbContext.InventoryItems => InventoryItems;
    IQueryable<Domain.Cart.Entities.Cart> IApplicationDbContext.Carts => Carts;
    IQueryable<Order> IApplicationDbContext.Orders => Orders;
    IQueryable<Payment> IApplicationDbContext.Payments => Payments;
    IQueryable<Customer> IApplicationDbContext.Customers => Customers;
    IQueryable<Seller> IApplicationDbContext.Sellers => Sellers;
    IQueryable<Notification> IApplicationDbContext.Notifications => Notifications;

    public new void Add<TEntity>(TEntity entity) where TEntity : class => Set<TEntity>().Add(entity);
    public new void Remove<TEntity>(TEntity entity) where TEntity : class => Set<TEntity>().Remove(entity);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdifyDbContext).Assembly);
    }
}