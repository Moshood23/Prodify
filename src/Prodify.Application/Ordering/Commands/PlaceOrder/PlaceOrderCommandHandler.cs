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

    public PlaceOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
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

        foreach (var cartItem in cart.Items)
        {
            var info = variantProductInfo.FirstOrDefault(x => x.VariantId == cartItem.ProductVariantId);

            if (info is null)
                throw new NotFoundException("ProductVariant", cartItem.ProductVariantId);

            lineItems.Add((info.SellerId, cartItem.ProductVariantId, info.Name, cartItem.Quantity, Money.Create(cartItem.UnitPrice)));
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

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}