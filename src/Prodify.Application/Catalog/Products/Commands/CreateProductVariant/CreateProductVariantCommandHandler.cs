using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Products.Commands.CreateProductVariant;

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductVariantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            throw new NotFoundException("Product", request.ProductId);

        var variant = ProductVariant.Create(
            request.ProductId,
            request.Sku,
            request.Price,
            request.Name,
            request.CompareAtPrice,
            request.Weight);

        _context.Add(variant);

        await _context.SaveChangesAsync(cancellationToken);

        return variant.Id;
    }
}