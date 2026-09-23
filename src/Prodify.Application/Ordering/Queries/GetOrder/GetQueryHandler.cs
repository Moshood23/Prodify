using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Ordering.Queries.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOrderQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
             .Include(o => o.Items)
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        // Customers only see their own orders; report someone else's order as "not found".
        if (order is null || (!_currentUser.IsAdmin() && order.CustomerId != _currentUser.CustomerId))
            throw new NotFoundException("Order", request.Id);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber.Value,
            CustomerId = order.CustomerId,
            IsPaid = order.IsPaid,
            Status = order.Status.ToString(),
            Total = order.Total.Amount,
            SellerOrders = order.SellerOrders.Select(so => new SellerOrderDto
            {
                Id = so.Id,
                SellerId = so.SellerId,
                Status = so.Status.ToString(),
                Total = so.Total.Amount,
                Items = so.Items.Select(i => new SellerOrderItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice.Amount
                }).ToList()
            }).ToList()
        };
    }
}