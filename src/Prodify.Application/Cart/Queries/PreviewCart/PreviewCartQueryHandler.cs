using MediatR;
using Prodify.Application.Cart.Common;
using Prodify.Application.Cart.Queries.GetCart;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Cart.Queries.PreviewCart;

public class PreviewCartQueryHandler : IRequestHandler<PreviewCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;

    public PreviewCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> Handle(PreviewCartQuery request, CancellationToken cancellationToken)
    {
        // The same variant twice counts as one line.
        var lines = request.Items
            .GroupBy(i => i.ProductVariantId)
            .Select(g => new CartLine(g.Key, g.Key, Math.Min(g.Sum(i => i.Quantity), CartRules.MaxQuantityPerItem), 0))
            .ToList();

        var variants = await _context.GetCartVariantsAsync(lines.Select(l => l.ProductVariantId).ToList(), cancellationToken);

        return CartPricing.Build(null, lines, variants);
    }
}
