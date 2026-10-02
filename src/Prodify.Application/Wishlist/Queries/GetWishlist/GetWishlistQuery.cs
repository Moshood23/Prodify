using MediatR;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Wishlist.Queries.GetWishlist;

// The customer's saved products, most recently saved first.
public class GetWishlistQuery : IRequest<PaginatedList<ProductSummaryDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
