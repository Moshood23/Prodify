using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateBrandCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Brand", request.Id);

        await _context.EnsureBrandNameIsFreeAsync(brand.Id, request.Name, cancellationToken);

        brand.Update(
            request.Name,
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim());
    }
}