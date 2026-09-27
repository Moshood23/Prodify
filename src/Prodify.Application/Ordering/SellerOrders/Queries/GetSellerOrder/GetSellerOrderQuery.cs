using MediatR;
using Prodify.Application.Ordering.SellerOrders.Common;

namespace Prodify.Application.Ordering.SellerOrders.Queries.GetSellerOrder;

public class GetSellerOrderQuery : IRequest<SellerOrderDetailsDto>
{
    public Guid Id { get; set; }
}