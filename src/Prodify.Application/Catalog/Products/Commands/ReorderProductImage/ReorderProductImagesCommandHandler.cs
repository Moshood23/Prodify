using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Catalog.Products.Commands.ReorderProductImages;

public class ReorderProductImagesCommandHandler : IRequestHandler<ReorderProductImagesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReorderProductImagesCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ReorderProductImagesCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null || !_currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException("Product", request.ProductId);

        // Someone may have added or removed a photo in another tab.
        var current = product.Images.Select(i => i.Id).ToHashSet();
        if (request.ImageIds.Count != current.Count || !request.ImageIds.All(current.Contains))
            throw new ConflictException("The photos have changed. Reload the page and try again.");

        product.ReorderImages(request.ImageIds);
    }
}