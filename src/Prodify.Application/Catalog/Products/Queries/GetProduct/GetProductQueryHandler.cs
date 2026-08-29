using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Products.Queries.GetProduct;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                IsActive = p.IsActive,
                ImageUrls = p.Images.Select(i => i.Url).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        return product;
    }
}