using MediatR;

namespace Prodify.Application.Catalog.Brands.Commands.UpdateBrand;

public class UpdateBrandCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
}