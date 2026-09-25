using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.RejectSeller;

public class RejectSellerCommandHandler : IRequestHandler<RejectSellerCommand>
{
    private readonly IApplicationDbContext _context;

    public RejectSellerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Reject(request.Reason);
    }
}