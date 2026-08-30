using MediatR;
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
        var brand = Brand.Create(request.Name, request.Description, request.LogoUrl);

        _context.Add(brand);

        await _context.SaveChangesAsync(cancellationToken);

        return brand.Id;
    }
}