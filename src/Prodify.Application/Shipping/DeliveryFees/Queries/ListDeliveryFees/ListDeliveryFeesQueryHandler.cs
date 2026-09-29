using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.DeliveryFees.Queries.ListDeliveryFees;

public class ListDeliveryFeesQueryHandler : IRequestHandler<ListDeliveryFeesQuery, List<DeliveryFeeDto>>
{
    private readonly IApplicationDbContext _context;

    public ListDeliveryFeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<DeliveryFeeDto>> Handle(ListDeliveryFeesQuery request, CancellationToken cancellationToken) =>
        _context.DeliveryFees
            .OrderBy(f => f.State)
            .Select(f => new DeliveryFeeDto { State = f.State, Fee = f.Amount })
            .ToListAsync(cancellationToken);
}
