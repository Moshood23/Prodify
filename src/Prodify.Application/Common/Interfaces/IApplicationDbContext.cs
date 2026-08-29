using Prodify.Domain.Cart.Entities;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Customers.Entities;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Payments.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Product> Products { get; }
    IQueryable<ProductVariant> ProductVariants { get; }
    IQueryable<Category> Categories { get; }
    IQueryable<Brand> Brands { get; }

    IQueryable<Warehouse> Warehouses { get; }
    IQueryable<InventoryItem> InventoryItems { get; }

    IQueryable<Cart> Carts { get; }

    IQueryable<Order> Orders { get; }

    IQueryable<Payment> Payments { get; }

    IQueryable<Customer> Customers { get; }

    IQueryable<Seller> Sellers { get; }

    IQueryable<Notification> Notifications { get; }

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}