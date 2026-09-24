using MediatR;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Catalog.Products.Queries.ListProducts;

public class ListProductsQuery : IRequest<PaginatedList<ProductSummaryDto>>
{
    // Free-text search over product name, brand and category.
    public string? Search { get; set; }

    // Includes products in the category's sub-categories.
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? SellerId { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // newest (default), price_asc, price_desc, name
    public string? Sort { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}