using MediatR;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Queries.GetSellerDetails;

// Admin: everything about one seller.
public class GetSellerDetailsQuery : IRequest<SellerDetailsDto>
{
    public Guid Id { get; set; }
}

public class SellerDetailsDto
{
    public SellerProfileDto Profile { get; set; } = null!;
    public int ProductCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int OrderCount { get; set; }

    // Value of the seller's orders that were not cancelled.
    public decimal TotalSales { get; set; }
}