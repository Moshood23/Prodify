using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Payouts.Commands.SavePayoutAccount;

public class SavePayoutAccountCommandHandler : IRequestHandler<SavePayoutAccountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SavePayoutAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SavePayoutAccountCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();

        var account = await _context.PayoutAccounts.FirstOrDefaultAsync(a => a.SellerId == sellerId, cancellationToken);

        // A payout already requested keeps the account it was requested with.
        if (account is null)
            _context.Add(PayoutAccount.Create(sellerId, request.BankName, request.AccountNumber, request.AccountName));
        else
            account.Update(request.BankName, request.AccountNumber, request.AccountName);
    }
}
