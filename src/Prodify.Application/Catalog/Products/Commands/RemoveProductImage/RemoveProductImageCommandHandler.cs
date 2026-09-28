using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Catalog.Products.Commands.RemoveProductImage;

public class RemoveProductImageCommandHandler : IRequestHandler<RemoveProductImageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _fileStorage;

    public RemoveProductImageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IFileStorage fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null || !_currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException("Product", request.ProductId);

        var image = product.Images.FirstOrDefault(i => i.Id == request.ImageId)
            ?? throw new NotFoundException("ProductImage", request.ImageId);

        product.RemoveImage(request.ImageId);

        // Uploaded photos are deleted from storage too; pasted links are left alone.
        await _fileStorage.DeleteAsync(image.Url, cancellationToken);
    }
}