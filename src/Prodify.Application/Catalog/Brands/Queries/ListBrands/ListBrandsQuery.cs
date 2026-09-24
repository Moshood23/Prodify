using MediatR;

namespace Prodify.Application.Catalog.Brands.Queries.ListBrands;

public class ListBrandsQuery : IRequest<List<BrandDto>>
{
}