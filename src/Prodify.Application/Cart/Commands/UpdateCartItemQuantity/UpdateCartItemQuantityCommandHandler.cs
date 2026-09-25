using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Cart.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCartItemQuantityCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        var item = cart?.Items.FirstOrDefault(i => i.Id == request.CartItemId);

        if (cart is null || item is null)
            throw new NotFoundException("CartItem", request.CartItemId);

        // Lowering the quantity is always allowed; raising it needs the stock.
        if (request.Quantity > item.Quantity)
        {
            var variants = await _context.GetCartVariantsAsync(new[] { item.ProductVariantId }, cancellationToken);
            var available = variants.TryGetValue(item.ProductVariantId, out var variant) && variant.IsAvailable
                ? variant.AvailableQuantity
                : 0;

            if (request.Quantity > available)
                throw new BusinessRuleException(available == 0
                    ? "This item is out of stock."
                    : $"Only {available} left in stock.");
        }

        cart.UpdateItemQuantity(request.CartItemId, request.Quantity);
    }
}