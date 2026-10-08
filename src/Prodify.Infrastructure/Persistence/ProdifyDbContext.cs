using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
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
using Prodify.Domain.Shipping.Entities;
using Prodify.Infrastructure.Identity;
using Prodify.Infrastructure.Messaging.Outbox;


namespace Prodify.Infrastructure.Persistence;

public class ProdifyDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext, IDataProtectionKeyContext
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
    public DbSet<Domain.Cart.Entities.CartItem> CartItems => Set<Domain.Cart.Entities.CartItem>();

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

    public DbSet<Prodify.Domain.Promotions.Entities.Voucher> Vouchers => Set<Prodify.Domain.Promotions.Entities.Voucher>();

    // Shipping
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<DeliveryFee> DeliveryFees => Set<DeliveryFee>();

    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Payouts
    public DbSet<Domain.Payouts.Entities.PlatformSettings> PlatformSettings => Set<Domain.Payouts.Entities.PlatformSettings>();
    public DbSet<Domain.Payouts.Entities.SellerEarning> SellerEarnings => Set<Domain.Payouts.Entities.SellerEarning>();
    public DbSet<Domain.Payouts.Entities.PayoutAccount> PayoutAccounts => Set<Domain.Payouts.Entities.PayoutAccount>();
    public DbSet<Domain.Payouts.Entities.SellerPayout> SellerPayouts => Set<Domain.Payouts.Entities.SellerPayout>();

    // Messaging
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Prodify.Infrastructure.Email.EmailMessage> EmailMessages => Set<Prodify.Infrastructure.Email.EmailMessage>();
    // Identity
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Keys that sign password-reset links; kept here so links stay valid across restarts and deploys.
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<SellerOrder> SellerOrders => Set<SellerOrder>();
    // IApplicationDbContext explicit IQueryable projections
    IQueryable<Product> IApplicationDbContext.Products => Products;
    IQueryable<ProductVariant> IApplicationDbContext.ProductVariants => ProductVariants;
    IQueryable<Category> IApplicationDbContext.Categories => Categories;
    IQueryable<Brand> IApplicationDbContext.Brands => Brands;
    IQueryable<Warehouse> IApplicationDbContext.Warehouses => Warehouses;
    IQueryable<InventoryItem> IApplicationDbContext.InventoryItems => InventoryItems;
    IQueryable<Domain.Cart.Entities.Cart> IApplicationDbContext.Carts => Carts;
    IQueryable<Domain.Cart.Entities.CartItem> IApplicationDbContext.CartItems => CartItems;
    IQueryable<Order> IApplicationDbContext.Orders => Orders;
    IQueryable<Payment> IApplicationDbContext.Payments => Payments;
    IQueryable<Customer> IApplicationDbContext.Customers => Customers;
    IQueryable<Seller> IApplicationDbContext.Sellers => Sellers;
    IQueryable<Notification> IApplicationDbContext.Notifications => Notifications;
    IQueryable<Prodify.Domain.Promotions.Entities.Voucher> IApplicationDbContext.Vouchers => Vouchers;
    IQueryable<Domain.Shipping.Entities.Shipment> IApplicationDbContext.Shipments => Shipments;
    IQueryable<DeliveryFee> IApplicationDbContext.DeliveryFees => DeliveryFees;
    IQueryable<ProductReview> IApplicationDbContext.ProductReviews => ProductReviews;
    IQueryable<WishlistItem> IApplicationDbContext.WishlistItems => WishlistItems;
    IQueryable<Domain.Payouts.Entities.PlatformSettings> IApplicationDbContext.PlatformSettings => PlatformSettings;
    IQueryable<Domain.Payouts.Entities.SellerEarning> IApplicationDbContext.SellerEarnings => SellerEarnings;
    IQueryable<Domain.Payouts.Entities.PayoutAccount> IApplicationDbContext.PayoutAccounts => PayoutAccounts;
    IQueryable<Domain.Payouts.Entities.SellerPayout> IApplicationDbContext.SellerPayouts => SellerPayouts;
    IQueryable<SellerOrder> IApplicationDbContext.SellerOrders => SellerOrders;
    public new void Add<TEntity>(TEntity entity) where TEntity : class => Set<TEntity>().Add(entity);
    public new void Remove<TEntity>(TEntity entity) where TEntity : class => Set<TEntity>().Remove(entity);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdifyDbContext).Assembly);
    }
}