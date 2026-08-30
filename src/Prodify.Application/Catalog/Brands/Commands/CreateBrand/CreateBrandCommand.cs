using MediatR;

namespace Prodify.Application.Catalog.Brands.Commands.CreateBrand;

public class CreateBrandCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
}