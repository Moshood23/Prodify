using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Cart.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveCartItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

        if (cart is null)
            throw new NotFoundException("Cart", request.CartId);

        cart.RemoveItem(request.CartItemId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}