using MediatR;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Sellers.Common;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Commands.ReapplySeller;

public class ReapplySellerCommandHandler : IRequestHandler<ReapplySellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReapplySellerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ReapplySellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(_currentUser.GetRequiredSellerId(), cancellationToken);

        if (seller.Status != SellerStatus.Rejected)
            throw new BusinessRuleException("Only a rejected application can be sent again.");

        seller.Reapply(request.BusinessName, request.PhoneNumber, request.Description);
    }
}