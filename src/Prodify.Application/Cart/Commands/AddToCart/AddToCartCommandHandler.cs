using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Cart.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddToCartCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var variants = await _context.GetCartVariantsAsync(new[] { request.ProductVariantId }, cancellationToken);

        // Hidden or removed products can't be added, and look the same as ones that never existed.
        if (!variants.TryGetValue(request.ProductVariantId, out var variant) || !variant.IsAvailable)
            throw new NotFoundException("ProductVariant", request.ProductVariantId);

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (cart is null)
        {
            cart = Domain.Cart.Entities.Cart.CreateForCustomer(customerId);
            _context.Add(cart);
        }

        var alreadyInCart = cart.Items.FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId)?.Quantity ?? 0;
        var newQuantity = alreadyInCart + request.Quantity;

        if (newQuantity > CartRules.MaxQuantityPerItem)
            throw new BusinessRuleException(
                $"You can buy at most {CartRules.MaxQuantityPerItem} of '{variant.ProductName}' per order. You already have {alreadyInCart} in your cart.");

        if (newQuantity > variant.AvailableQuantity)
            throw new BusinessRuleException(variant.AvailableQuantity == 0
                ? $"'{variant.ProductName}' is out of stock."
                : $"Only {variant.AvailableQuantity} of '{variant.ProductName}' left in stock. You already have {alreadyInCart} in your cart.");

        cart.AddItem(request.ProductVariantId, request.Quantity, variant.Price);

        return cart.Id;
    }
}