using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Application.Ordering.Commands.PlaceOrder;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    // How long card orders hold their stock while waiting for payment.
    public const int PaymentWindowMinutes = 30;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PlaceOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();
        var paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, ignoreCase: true);

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (cart is null || !cart.Items.Any())
            throw new BusinessRuleException("Your cart is empty.");

        var variants = await _context.GetCartVariantsAsync(
            cart.Items.Select(i => i.ProductVariantId).ToList(), cancellationToken);

        var lineItems = new List<(Guid SellerId, Guid ProductVariantId, string ProductName, int Quantity, Money UnitPrice)>();
        var stockToReserve = new List<(InventoryItem Item, int Quantity)>();

        foreach (var cartItem in cart.Items)
        {
            if (!variants.TryGetValue(cartItem.ProductVariantId, out var variant) || !variant.IsAvailable)
                throw new BusinessRuleException("An item in your cart is no longer available. Remove it to continue.");

            var inventoryItem = await SelectWarehouseWithStockAsync(cartItem.ProductVariantId, cartItem.Quantity, cancellationToken)
                ?? throw new BusinessRuleException(variant.AvailableQuantity == 0
                    ? $"'{variant.ProductName}' is out of stock. Remove it to continue."
                    : $"Only {variant.AvailableQuantity} of '{variant.ProductName}' left. Lower the quantity to continue.");

            stockToReserve.Add((inventoryItem, cartItem.Quantity));

            // Customers pay today's price, not the price when they added the item.
            var name = variant.VariantName is null ? variant.ProductName : $"{variant.ProductName} ({variant.VariantName})";
            lineItems.Add((variant.SellerId, cartItem.ProductVariantId, name, cartItem.Quantity, Money.Create(variant.Price)));
        }

        var shippingAddress = OrderAddress.Create(
            request.RecipientName,
            request.AddressLine1,
            request.City,
            request.State,
            request.Country,
            request.PhoneNumber,
            request.AddressLine2,
            request.PostalCode);

        var order = Order.Place(customerId, shippingAddress, lineItems, paymentMethod);
        _context.Add(order);

        foreach (var (item, quantity) in stockToReserve)
        {
            var reservation = item.Reserve(quantity, TimeSpan.FromMinutes(PaymentWindowMinutes), order.Id);

            // Pay on delivery: nothing left to wait for, the stock is committed to this order now.
            if (paymentMethod == PaymentMethod.PayOnDelivery)
                item.ConfirmReservation(reservation.Id);
        }

        cart.Clear();

        return order.Id;
    }

    private async Task<InventoryItem?> SelectWarehouseWithStockAsync(
        Guid productVariantId, int requiredQuantity, CancellationToken cancellationToken)
    {
        var candidates = await _context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.ProductVariantId == productVariantId)
            .OrderBy(i => i.WarehouseId)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(i => i.AvailableQuantity >= requiredQuantity);
    }
}