using MediatR;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Catalog.Products.Queries.ListManagedProducts;

// Seller Centre product list. Sellers always see their own products;
// admins pass the seller they want to look at.
public class ListManagedProductsQuery : IRequest<PaginatedList<ManagedProductSummaryDto>>
{
    public Guid? SellerId { get; set; }
    public string? Search { get; set; }

    // "active" or "inactive"; empty for all.
    public string? Filter { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}