using MediatR;
using Prodify.Application.Catalog.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Brands.Commands.CreateBrand;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateBrandCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        await _context.EnsureBrandNameIsFreeAsync(null, request.Name, cancellationToken);

        var brand = Brand.Create(
            request.Name,
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim());

        _context.Add(brand);

        return brand.Id;
    }
}