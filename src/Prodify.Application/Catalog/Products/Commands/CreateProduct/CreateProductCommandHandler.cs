using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var sellerId = ResolveSellerId(request);

        var seller = await _context.Sellers
            .FirstOrDefaultAsync(s => s.Id == sellerId, cancellationToken);

        if (seller is null)
            throw new NotFoundException("Seller", sellerId);

        if (seller.Status != SellerStatus.Approved)
            throw new BusinessRuleException("The seller account must be approved before products can be listed.");

        var product = Product.Create(
            request.Name,
            request.Description,
            request.CategoryId,
            sellerId,
            request.BrandId);

        _context.Add(product);

        return product.Id;
    }

    private Guid ResolveSellerId(CreateProductCommand request)
    {
        if (_currentUser.IsAdmin())
        {
            return request.SellerId
                ?? throw new BusinessRuleException("SellerId is required when an admin creates a product.");
        }

        return _currentUser.GetRequiredSellerId();
    }
}