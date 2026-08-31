using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Cart.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;

    public GetCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var query = request.CustomerId.HasValue
            ? _context.Carts.Where(c => c.CustomerId == request.CustomerId)
            : _context.Carts.Where(c => c.SessionId == request.SessionId);

        var cart = await query
            .Select(c => new CartDto
            {
                Id = c.Id,
                Total = c.Total,
                Items = c.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null)
            throw new NotFoundException("Cart", request.CustomerId?.ToString() ?? request.SessionId ?? "unknown");

        return cart;
    }
}