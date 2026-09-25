using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Common;
using Prodify.Domain.Ordering.Entities;

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

        var variantIds = order.SellerOrders.SelectMany(so => so.Items).Select(i => i.ProductVariantId).Distinct().ToList();
        var products = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                ProductId = p.Id,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.VariantId, cancellationToken);

        var sellerIds = order.SellerOrders.Select(so => so.SellerId).Distinct().ToList();
        var sellerNames = await _context.Sellers
            .Where(s => sellerIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.BusinessName, cancellationToken);

        var canPay = !order.IsPaid
            && order.PaymentMethod == PaymentMethod.Card
            && order.Status != OrderStatus.Cancelled
            && await _context.HasActiveReservationsAsync(order.Id, cancellationToken);

        var address = order.ShippingAddress;

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber.Value,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            IsPaid = order.IsPaid,
            PaymentMethod = order.PaymentMethod.ToString(),
            Status = order.Status.ToString(),
            Total = order.Total.Amount,
            CanCancel = OrderRules.CanBeCancelled(order),
            CanPay = canPay,
            ShippingAddress = new OrderAddressDto
            {
                RecipientName = address.RecipientName,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                PhoneNumber = address.PhoneNumber
            },
            SellerOrders = order.SellerOrders.Select(so => new SellerOrderDto
            {
                Id = so.Id,
                SellerId = so.SellerId,
                SellerName = sellerNames.GetValueOrDefault(so.SellerId) ?? "Seller",
                Status = so.Status.ToString(),
                Total = so.Total.Amount,
                Items = so.Items.Select(i => new SellerOrderItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductId = products.GetValueOrDefault(i.ProductVariantId)?.ProductId,
                    ProductName = i.ProductName,
                    ImageUrl = products.GetValueOrDefault(i.ProductVariantId)?.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice.Amount,
                    Subtotal = i.UnitPrice.Amount * i.Quantity
                }).ToList()
            }).ToList()
        };
    }
}