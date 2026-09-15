using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Application.Ordering.Commands.PlaceOrder;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private const int DefaultReservationExpiryMinutes = 30;

    public PlaceOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is null || !cart.Items.Any())
            throw new BusinessRuleException("Cart is empty or does not exist.");

        var variantIds = cart.Items.Select(i => i.ProductVariantId).ToList();

        var variantProductInfo = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .Join(_context.Products,
                v => v.ProductId,
                p => p.Id,
                (v, p) => new { VariantId = v.Id, p.SellerId, p.Name })
            .ToListAsync(cancellationToken);

        var lineItems = new List<(Guid SellerId, Guid ProductVariantId, string ProductName, int Quantity, Money UnitPrice)>();
        var reservations = new List<(Domain.Inventory.Entities.InventoryItem Item, int Quantity)>();

        foreach (var cartItem in cart.Items)
        {
            var info = variantProductInfo.FirstOrDefault(x => x.VariantId == cartItem.ProductVariantId);

            if (info is null)
                throw new NotFoundException("ProductVariant", cartItem.ProductVariantId);

            var inventoryItem = await SelectWarehouseWithStockAsync(cartItem.ProductVariantId, cartItem.Quantity, cancellationToken);

            if (inventoryItem is null)
                throw new BusinessRuleException($"Insufficient stock for '{info.Name}'.");

            reservations.Add((inventoryItem, cartItem.Quantity));

            lineItems.Add((info.SellerId, cartItem.ProductVariantId, info.Name, cartItem.Quantity, Money.Create(cartItem.UnitPrice)));
        }

        foreach (var (item, quantity) in reservations)
        {
            item.Reserve(quantity, TimeSpan.FromMinutes(DefaultReservationExpiryMinutes));
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

        var order = Order.Place(request.CustomerId, shippingAddress, lineItems);

        _context.Add(order);

        cart.Clear();

        return order.Id;
    }

    private async Task<Domain.Inventory.Entities.InventoryItem?> SelectWarehouseWithStockAsync(
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