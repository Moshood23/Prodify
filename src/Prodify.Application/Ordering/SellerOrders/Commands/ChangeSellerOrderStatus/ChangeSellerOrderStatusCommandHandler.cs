using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Common;
using Prodify.Application.Ordering.SellerOrders.Common;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Commands.ChangeSellerOrderStatus;

public class ChangeSellerOrderStatusCommandHandler : IRequestHandler<ChangeSellerOrderStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;

    public ChangeSellerOrderStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPaymentService paymentService)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task Handle(ChangeSellerOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var (order, sellerOrder) = await _context.GetManagedSellerOrderAsync(_currentUser, request.SellerOrderId, cancellationToken);
        var status = Enum.Parse<SellerOrderStatus>(request.Status, ignoreCase: true);

        switch (status)
        {
            case SellerOrderStatus.Confirmed:
                Ensure(SellerOrderRules.CanConfirm(sellerOrder, order), sellerOrder, "confirmed");
                sellerOrder.TransitionTo(SellerOrderStatus.Confirmed);
                break;

            case SellerOrderStatus.Packed:
                Ensure(SellerOrderRules.CanPack(sellerOrder), sellerOrder, "packed");
                sellerOrder.TransitionTo(SellerOrderStatus.Packed);
                break;

            case SellerOrderStatus.Shipped:
                Ensure(SellerOrderRules.CanShip(sellerOrder), sellerOrder, "shipped");
                await ShipAsync(sellerOrder, request.Carrier!, request.TrackingNumber, cancellationToken);
                break;

            case SellerOrderStatus.Delivered:
                Ensure(SellerOrderRules.CanDeliver(sellerOrder), sellerOrder, "marked as delivered");
                await MarkShipmentDeliveredAsync(sellerOrder.Id, cancellationToken);
                order.MarkSellerOrderDelivered(sellerOrder.Id);
                break;

            case SellerOrderStatus.Cancelled:
                Ensure(SellerOrderRules.CanCancel(sellerOrder), sellerOrder, "cancelled");

                // The customer gets this part's money back (refund first: if it fails, nothing changes).
                await _context.RefundIfPaidAsync(_paymentService, order, OrderRefunds.RefundFor(order, new[] { sellerOrder }), cancellationToken);
                sellerOrder.TransitionTo(SellerOrderStatus.Cancelled, request.Reason!.Trim());

                var variantIds = sellerOrder.Items.Select(i => i.ProductVariantId).ToList();
                await _context.ReleaseSellerOrderStockAsync(order.Id, variantIds, cancellationToken);
                break;
        }

        // Pay on delivery: the last delivery (or a cancel that leaves only delivered parts) means the cash is in.
        _context.RecordCashOnDeliveryIfComplete(order);
    }

    // The package leaves: record it as a shipment so it can be tracked.
    private async Task ShipAsync(SellerOrder sellerOrder, string carrier, string? trackingNumber, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.SellerOrderId == sellerOrder.Id && s.Status == ShipmentStatus.Pending, cancellationToken);

        if (shipment is null)
        {
            shipment = Shipment.Create(sellerOrder.Id, sellerOrder.Items.Select(i => (i.ProductVariantId, i.Quantity)));
            _context.Add(shipment);
        }

        shipment.Ship(carrier, trackingNumber);

        var note = string.IsNullOrWhiteSpace(trackingNumber)
            ? $"Sent with {carrier.Trim()}"
            : $"Sent with {carrier.Trim()}, tracking number {trackingNumber.Trim()}";
        sellerOrder.TransitionTo(SellerOrderStatus.Shipped, note);
    }

    private async Task MarkShipmentDeliveredAsync(Guid sellerOrderId, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .Where(s => s.SellerOrderId == sellerOrderId && s.Status != ShipmentStatus.Delivered && s.Status != ShipmentStatus.Failed)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        shipment?.UpdateStatus(ShipmentStatus.Delivered, "Package delivered");
    }

    private static void Ensure(bool allowed, SellerOrder sellerOrder, string action)
    {
        if (!allowed)
            throw new BusinessRuleException($"This order can't be {action} now (status: {sellerOrder.Status}).");
    }
}