using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Shipping.Commands.CreateShipment;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateShipmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var sellerOrder = await _context.SellerOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == request.SellerOrderId, cancellationToken);

        if (sellerOrder is null)
            throw new NotFoundException("SellerOrder", request.SellerOrderId);

        var items = sellerOrder.Items.Select(i => (i.ProductVariantId, i.Quantity));

        var shipment = Shipment.Create(sellerOrder.Id, items);

        _context.Add(shipment);

        return shipment.Id;
    }
}