using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Application.Wishlist.Commands.SaveToWishlist;

public class SaveToWishlistCommandHandler : IRequestHandler<SaveToWishlistCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SaveToWishlistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SaveToWishlistCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        if (!await _context.Products.Where(p => p.Id == request.ProductId).WherePubliclyVisible(_context).AnyAsync(cancellationToken))
            throw new NotFoundException("Product", request.ProductId);

        var saved = _context.WishlistItems.Where(w => w.CustomerId == customerId);

        if (await saved.AnyAsync(w => w.ProductId == request.ProductId, cancellationToken))
            return;

        if (await saved.CountAsync(cancellationToken) >= WishlistItem.MaxItemsPerCustomer)
            throw new BusinessRuleException($"Your wishlist is full ({WishlistItem.MaxItemsPerCustomer} items). Remove something to save this.");

        _context.Add(WishlistItem.Create(customerId, request.ProductId));
    }
}
