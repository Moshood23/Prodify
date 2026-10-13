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

    IQueryable<Domain.Cart.Entities.Cart> Carts { get; }
    IQueryable<Domain.Cart.Entities.CartItem> CartItems { get; }
    IQueryable<Order> Orders { get; }

    IQueryable<Payment> Payments { get; }

    IQueryable<Customer> Customers { get; }

    IQueryable<Seller> Sellers { get; }

    IQueryable<SellerOrder> SellerOrders { get; }

    IQueryable<Domain.Shipping.Entities.Shipment> Shipments { get; }

    IQueryable<Domain.Shipping.Entities.DeliveryFee> DeliveryFees { get; }

    IQueryable<ProductReview> ProductReviews { get; }

    IQueryable<WishlistItem> WishlistItems { get; }
    IQueryable<ProductView> ProductViews { get; }

    IQueryable<Domain.Payouts.Entities.PlatformSettings> PlatformSettings { get; }
    IQueryable<Domain.Payouts.Entities.SellerEarning> SellerEarnings { get; }
    IQueryable<Domain.Payouts.Entities.PayoutAccount> PayoutAccounts { get; }
    IQueryable<Domain.Payouts.Entities.SellerPayout> SellerPayouts { get; }

    IQueryable<Notification> Notifications { get; }

    IQueryable<Domain.Promotions.Entities.Voucher> Vouchers { get; }
    IQueryable<Domain.Promotions.Entities.FlashSale> FlashSales { get; }
    IQueryable<Domain.Promotions.Entities.FlashSaleClaim> FlashSaleClaims { get; }

    IQueryable<Domain.Messaging.Entities.Conversation> Conversations { get; }
    IQueryable<Domain.Messaging.Entities.ConversationMessage> ConversationMessages { get; }

    IQueryable<StoreCreditEntry> StoreCreditEntries { get; }

    IQueryable<Domain.Returns.Entities.ReturnRequest> ReturnRequests { get; }

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    Task LockPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    // The same for a customer, e.g. while spending their store credit.
    Task LockCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    // And on a flash sale, while an order takes some of its units.
    Task LockFlashSaleAsync(Guid flashSaleId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
