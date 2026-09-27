using MediatR;
using Prodify.Application.Ordering.SellerOrders.Common;

namespace Prodify.Application.Ordering.SellerOrders.Queries.GetSellerOrderCounts;

// Sellers get their own counts; admins pass the seller.
public class GetSellerOrderCountsQuery : IRequest<SellerOrderCountsDto>
{
    public Guid? SellerId { get; set; }
}