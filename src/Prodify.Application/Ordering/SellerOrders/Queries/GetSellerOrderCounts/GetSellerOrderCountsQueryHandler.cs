using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.SellerOrders.Common;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Queries.GetSellerOrderCounts;

public class GetSellerOrderCountsQueryHandler : IRequestHandler<GetSellerOrderCountsQuery, SellerOrderCountsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSellerOrderCountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SellerOrderCountsDto> Handle(GetSellerOrderCountsQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.IsAdmin()
            ? request.SellerId ?? throw new BusinessRuleException("Choose a seller to see their orders.")
            : _currentUser.GetRequiredSellerId();

        var counts = await _context.SellerOrders
            .Where(so => so.SellerId == sellerId)
            .Join(_context.Orders, so => so.OrderId, o => o.Id, (so, o) => new { so.Status, o.PaymentMethod, o.IsPaid })
            .Where(x => x.PaymentMethod == PaymentMethod.PayOnDelivery || x.IsPaid)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        return new SellerOrderCountsDto
        {
            ToConfirm = counts.GetValueOrDefault(SellerOrderStatus.Pending),
            ToPack = counts.GetValueOrDefault(SellerOrderStatus.Confirmed),
            ToShip = counts.GetValueOrDefault(SellerOrderStatus.Packed),
            Shipped = counts.GetValueOrDefault(SellerOrderStatus.Shipped),
            Delivered = counts.GetValueOrDefault(SellerOrderStatus.Delivered),
            Cancelled = counts.GetValueOrDefault(SellerOrderStatus.Cancelled)
        };
    }
}