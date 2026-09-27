using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Queries.GetOrder;

namespace Prodify.Application.Admin.Orders.Queries.GetAdminOrder;

public class GetAdminOrderQueryHandler : IRequestHandler<GetAdminOrderQuery, AdminOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public GetAdminOrderQueryHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<AdminOrderDto> Handle(GetAdminOrderQuery request, CancellationToken cancellationToken)
    {
        // Same view the customer gets (admins are allowed to see any order).
        var order = await _sender.Send(new GetOrderQuery { Id = request.Id }, cancellationToken);

        var customer = await _context.Customers
            .Where(c => c.Id == order.CustomerId)
            .Select(c => new AdminOrderCustomerDto
            {
                Id = c.Id,
                Name = c.FirstName + " " + c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new AdminOrderCustomerDto { Id = order.CustomerId, Name = "Deleted customer", Email = "" };

        var sellerOrderIds = order.SellerOrders.Select(so => so.Id).ToList();

        var history = await _context.SellerOrders
            .Where(so => sellerOrderIds.Contains(so.Id))
            .Select(so => new
            {
                so.Id,
                History = so.StatusHistory
                    .OrderBy(h => h.OccurredAt)
                    .Select(h => new AdminStatusHistoryDto { Status = h.Status.ToString(), Notes = h.Notes, OccurredAt = h.OccurredAt })
                    .ToList()
            })
            .ToDictionaryAsync(x => x.Id, x => x.History, cancellationToken);

        var shipments = await _context.Shipments
            .Where(s => sellerOrderIds.Contains(s.SellerOrderId))
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new { s.SellerOrderId, s.Carrier, s.TrackingNumber })
            .ToListAsync(cancellationToken);

        return new AdminOrderDto
        {
            Order = order,
            Customer = customer,
            SellerProgress = sellerOrderIds.Select(id =>
            {
                var shipment = shipments.FirstOrDefault(s => s.SellerOrderId == id);
                return new AdminSellerOrderProgressDto
                {
                    SellerOrderId = id,
                    Carrier = shipment?.Carrier,
                    TrackingNumber = shipment?.TrackingNumber,
                    History = history.GetValueOrDefault(id) ?? new()
                };
            }).ToList()
        };
    }
}