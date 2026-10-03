using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Cart.Commands.MergeGuestCart;

public class MergeGuestCartCommandHandler : IRequestHandler<MergeGuestCartCommand, List<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MergeGuestCartCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<string>> Handle(MergeGuestCartCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();
        var notes = new List<string>();

        var wanted = request.Items
            .GroupBy(i => i.ProductVariantId)
            .Select(g => (VariantId: g.Key, Quantity: g.Sum(i => i.Quantity)))
            .ToList();

        if (wanted.Count == 0)
            return notes;

        var variants = await _context.GetCartVariantsAsync(wanted.Select(w => w.VariantId).ToList(), cancellationToken);

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (cart is null)
        {
            cart = Domain.Cart.Entities.Cart.CreateForCustomer(customerId);
            _context.Add(cart);
        }

        foreach (var (variantId, quantity) in wanted)
        {
            // Hidden or removed while it sat in the browser.
            if (!variants.TryGetValue(variantId, out var variant) || !variant.IsAvailable)
            {
                notes.Add($"{variant?.ProductName ?? "An item"} is no longer for sale, so it wasn't added.");
                continue;
            }

            var alreadyInCart = cart.Items.FirstOrDefault(i => i.ProductVariantId == variantId)?.Quantity ?? 0;
            var room = Math.Min(CartRules.MaxQuantityPerItem, variant.AvailableQuantity) - alreadyInCart;
            var toAdd = Math.Min(quantity, room);

            if (toAdd <= 0)
            {
                if (alreadyInCart == 0)
                    notes.Add($"{variant.ProductName} is out of stock, so it wasn't added.");
                continue;
            }

            if (toAdd < quantity)
                notes.Add($"Only {toAdd} more of {variant.ProductName} could be added.");

            cart.AddItem(variantId, toAdd, variant.Price);
        }

        return notes;
    }
}
