using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.SuspendSeller;

public class SuspendSellerCommandHandler : IRequestHandler<SuspendSellerCommand>
{
    private readonly IApplicationDbContext _context;

    public SuspendSellerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspendSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Suspend(request.Reason);
    }
}