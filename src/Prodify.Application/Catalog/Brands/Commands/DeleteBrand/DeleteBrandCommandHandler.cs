using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Brands.Commands.DeleteBrand;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteBrandCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Brand", request.Id);

        var productCount = await _context.Products.CountAsync(p => p.BrandId == brand.Id, cancellationToken);
        if (productCount > 0)
            throw new BusinessRuleException($"'{brand.Name}' is used by {productCount} product(s). Change their brand first.");

        _context.Remove(brand);
    }
}