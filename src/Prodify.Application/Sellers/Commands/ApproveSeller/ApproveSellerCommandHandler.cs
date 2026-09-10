using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

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
        var seller = await _context.Sellers
            .FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken);

        if (seller is null)
            throw new NotFoundException("Seller", request.SellerId);

        seller.Approve();

        await _context.SaveChangesAsync(cancellationToken);
    }
}