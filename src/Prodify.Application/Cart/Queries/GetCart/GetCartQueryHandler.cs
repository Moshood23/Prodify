using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Cart.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCartQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        // A customer who has not added anything yet simply has an empty cart.
        if (cart is null)
            return new CartDto();

        var variants = await _context.GetCartVariantsAsync(
            cart.Items.Select(i => i.ProductVariantId).ToList(), cancellationToken);

        var items = cart.Items
            .Select(item =>
            {
                variants.TryGetValue(item.ProductVariantId, out var variant);
                var unitPrice = variant?.Price ?? item.UnitPrice;
                var isAvailable = variant?.IsAvailable ?? false;

                return new CartItemDto
                {
                    Id = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductId = variant?.ProductId ?? Guid.Empty,
                    ProductName = variant?.ProductName ?? "Product no longer available",
                    VariantName = variant?.VariantName,
                    ImageUrl = variant?.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Subtotal = unitPrice * item.Quantity,
                    AvailableQuantity = variant?.AvailableQuantity ?? 0,
                    IsAvailable = isAvailable,
                    InStock = isAvailable && (variant?.AvailableQuantity ?? 0) >= item.Quantity
                };
            })
            .OrderBy(i => i.ProductName)
            .ToList();

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            ItemCount = items.Sum(i => i.Quantity),
            // Items that can't be bought don't count towards what the customer will pay.
            Total = items.Where(i => i.IsAvailable).Sum(i => i.Subtotal),
            HasProblems = items.Any(i => !i.InStock)
        };
    }
}