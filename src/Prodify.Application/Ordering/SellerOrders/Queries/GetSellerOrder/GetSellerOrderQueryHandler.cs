using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.SellerOrders.Common;

namespace Prodify.Application.Ordering.SellerOrders.Queries.GetSellerOrder;

public class GetSellerOrderQueryHandler : IRequestHandler<GetSellerOrderQuery, SellerOrderDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSellerOrderQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SellerOrderDetailsDto> Handle(GetSellerOrderQuery request, CancellationToken cancellationToken)
    {
        var (order, sellerOrder) = await _context.GetManagedSellerOrderAsync(_currentUser, request.Id, cancellationToken);

        var variantIds = sellerOrder.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var products = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                v.SKU,
                ProductId = p.Id,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.VariantId, cancellationToken);

        var shipment = await _context.Shipments
            .Where(s => s.SellerOrderId == sellerOrder.Id)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SellerOrderShipmentDto
            {
                Carrier = s.Carrier,
                TrackingNumber = s.TrackingNumber,
                ShippedAt = s.ShippedAt,
                DeliveredAt = s.DeliveredAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        var address = order.ShippingAddress;

        return new SellerOrderDetailsDto
        {
            Id = sellerOrder.Id,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber.Value,
            CreatedAt = order.CreatedAt,
            Status = sellerOrder.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            IsPaid = order.IsPaid,
            Total = sellerOrder.Total.Amount,
            ShippingAddress = new SellerOrderAddressDto
            {
                RecipientName = address.RecipientName,
                PhoneNumber = address.PhoneNumber,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State
            },
            Items = sellerOrder.Items.Select(i =>
            {
                var product = products.GetValueOrDefault(i.ProductVariantId);
                return new SellerOrderLineDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductId = product?.ProductId,
                    ProductName = i.ProductName,
                    Sku = product?.SKU.Value,
                    ImageUrl = product?.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice.Amount,
                    Subtotal = i.UnitPrice.Amount * i.Quantity
                };
            }).ToList(),
            Shipment = shipment,
            History = sellerOrder.StatusHistory
                .OrderBy(h => h.OccurredAt)
                .Select(h => new SellerOrderHistoryDto { Status = h.Status.ToString(), Notes = h.Notes, OccurredAt = h.OccurredAt })
                .ToList(),
            CanConfirm = SellerOrderRules.CanConfirm(sellerOrder, order),
            CanPack = SellerOrderRules.CanPack(sellerOrder),
            CanShip = SellerOrderRules.CanShip(sellerOrder),
            CanDeliver = SellerOrderRules.CanDeliver(sellerOrder),
            CanCancel = SellerOrderRules.CanCancel(sellerOrder)
        };
    }
}