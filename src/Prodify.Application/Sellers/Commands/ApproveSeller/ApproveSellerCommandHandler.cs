using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.ApproveSeller;

public class ApproveSellerCommandHandler : IRequestHandler<ApproveSellerCommand>
{
    private readonly IApplicationDbContext _context;

    public ApproveSellerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ApproveSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Approve();
    }
}