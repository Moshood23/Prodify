using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Catalog.Products.Commands.UploadProductImage;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _fileStorage;

    public UploadProductImageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IFileStorage fileStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null || !_currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException("Product", request.ProductId);

        if (product.Images.Count >= ProductImageRules.MaxImagesPerProduct)
            throw new BusinessRuleException($"A product can have at most {ProductImageRules.MaxImagesPerProduct} photos.");

        // Read into memory (at most 5 MB) so the type can be checked before saving.
        using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);

        var extension = ProductImageRules.DetectExtension(buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, 16)));
        if (extension is null)
            throw new BusinessRuleException("Only JPG, PNG or WebP photos can be uploaded.");

        buffer.Position = 0;
        var url = await _fileStorage.SaveAsync(buffer, ProductImageRules.UploadFolder, extension, cancellationToken);

        var image = product.AddImage(url, string.IsNullOrWhiteSpace(request.AltText) ? null : request.AltText.Trim());

        return image.Id;
    }
}