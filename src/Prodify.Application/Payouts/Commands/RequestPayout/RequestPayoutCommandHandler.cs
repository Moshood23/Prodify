using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Payouts.Common;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Payouts.Commands.RequestPayout;

public class RequestPayoutCommandHandler : IRequestHandler<RequestPayoutCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RequestPayoutCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();

        var account = await _context.PayoutAccounts.FirstOrDefaultAsync(a => a.SellerId == sellerId, cancellationToken)
            ?? throw new BusinessRuleException("Add the bank account to pay into first.");

        // One request at a time; the database also enforces this.
        if (await _context.SellerPayouts.AnyAsync(p => p.SellerId == sellerId && p.Status == PayoutStatus.Requested, cancellationToken))
            throw new BusinessRuleException("You already have a payout waiting to be paid. You can ask again once it's done.");

        var balance = await _context.GetSellerBalanceAsync(sellerId, cancellationToken);
        if (request.Amount > balance.Available)
            throw new BusinessRuleException($"You can withdraw up to {Naira.Format(balance.Available)} right now.");

        var payout = SellerPayout.Request(sellerId, request.Amount, account);
        _context.Add(payout);

        return payout.Id;
    }
}
