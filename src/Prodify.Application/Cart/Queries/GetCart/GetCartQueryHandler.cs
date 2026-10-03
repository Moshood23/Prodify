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

        var lines = cart.Items.Select(i => new CartLine(i.Id, i.ProductVariantId, i.Quantity, i.UnitPrice));
        return CartPricing.Build(cart.Id, lines, variants);
    }
}
