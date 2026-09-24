using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Catalog.Products.Commands.AddProductImage;

public class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, Guid>
{
    private const int MaxImagesPerProduct = 10;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddProductImageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null || !_currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException("Product", request.ProductId);

        if (product.Images.Count >= MaxImagesPerProduct)
            throw new BusinessRuleException($"A product can have at most {MaxImagesPerProduct} images.");

        var image = product.AddImage(request.Url, request.AltText);

        return image.Id;
    }
}