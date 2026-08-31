using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Cart.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddToCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var variant = await _context.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId, cancellationToken);

        if (variant is null)
            throw new NotFoundException("ProductVariant", request.ProductVariantId);

        var cart = request.CustomerId.HasValue
            ? await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken)
            : await _context.Carts.FirstOrDefaultAsync(c => c.SessionId == request.SessionId, cancellationToken);

        if (cart is null)
        {
            cart = request.CustomerId.HasValue
                ? Domain.Cart.Entities.Cart.CreateForCustomer(request.CustomerId.Value)
                : Domain.Cart.Entities.Cart.CreateForGuest(request.SessionId!);

            _context.Add(cart);
        }

        cart.AddItem(request.ProductVariantId, request.Quantity, variant.Price);

        await _context.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}