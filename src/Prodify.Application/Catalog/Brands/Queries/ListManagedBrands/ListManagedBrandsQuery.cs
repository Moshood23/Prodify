using MediatR;

namespace Prodify.Application.Catalog.Brands.Queries.ListManagedBrands;

// Admin: every brand with how many products use it.
public class ListManagedBrandsQuery : IRequest<List<ManagedBrandDto>>
{
}

public class ManagedBrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public int ProductCount { get; set; }
}