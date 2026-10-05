using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Admin.Payouts.Commands.ProcessPayout;

public class ProcessPayoutCommandHandler : IRequestHandler<ProcessPayoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ProcessPayoutCommandHandler(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        var payout = await _context.SellerPayouts.FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken)
            ?? throw new NotFoundException("Payout", request.PayoutId);

        if (request.Paid)
            payout.MarkPaid(request.Reference!);
        else
            payout.Reject(request.Reason!);

        var seller = await _context.Sellers
            .Where(s => s.Id == payout.SellerId)
            .Select(s => new { s.BusinessName, s.Email })
            .FirstOrDefaultAsync(cancellationToken);

        if (seller is null)
            return;

        var amount = Naira.Format(payout.Amount);
        var account = $"{payout.BankName}, {payout.AccountName} ({payout.AccountNumber})";

        _emails.Enqueue(seller.Email, seller.BusinessName, request.Paid
            ? EmailLayout.Build(
                $"Your payout of {amount} has been sent",
                "Payout sent",
                new[]
                {
                    $"Hi {seller.BusinessName},",
                    $"We've sent {amount} to {account}.",
                    $"Bank reference: {payout.Reference}. It can take a few hours to show in your account.",
                },
                ("View earnings", _urls.Page("/seller/earnings")))
            : EmailLayout.Build(
                $"Your payout request of {amount} was not paid",
                "Payout not paid",
                new[]
                {
                    $"Hi {seller.BusinessName},",
                    $"We couldn't pay your request of {amount}.",
                    $"Reason: {payout.RejectReason}",
                    "The money is back in your available balance, so you can fix the problem and ask again.",
                },
                ("View earnings", _urls.Page("/seller/earnings"))));
    }
}
