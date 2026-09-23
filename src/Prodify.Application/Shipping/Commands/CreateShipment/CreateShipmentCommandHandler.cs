using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Shipping.Common;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Shipping.Commands.CreateShipment;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateShipmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var sellerOrder = await _context.SellerOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == request.SellerOrderId, cancellationToken);

        if (sellerOrder is null
            || !await ShipmentAccess.CanManageAsync(_context, _currentUser, sellerOrder.Id, cancellationToken))
            throw new NotFoundException("SellerOrder", request.SellerOrderId);

        var items = sellerOrder.Items.Select(i => (i.ProductVariantId, i.Quantity));

        var shipment = Shipment.Create(sellerOrder.Id, items);

        _context.Add(shipment);

        return shipment.Id;
    }
}