using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.ReinstateSeller;

public class ReinstateSellerCommandHandler : IRequestHandler<ReinstateSellerCommand>
{
    private readonly IApplicationDbContext _context;

    public ReinstateSellerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReinstateSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Reinstate();
    }
}