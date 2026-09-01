using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Ordering.Queries.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Where(o => o.Id == request.Id)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber.Value,
                CustomerId = o.CustomerId,
                IsPaid = o.IsPaid,
                Status = o.Status.ToString(),
                Total = o.Total.Amount,
                SellerOrders = o.SellerOrders.Select(so => new SellerOrderDto
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
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            throw new NotFoundException("Order", request.Id);

        return order;
    }
}