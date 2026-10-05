using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Admin.Payouts.Queries.ListAdminPayouts;

public class ListAdminPayoutsQueryHandler : IRequestHandler<ListAdminPayoutsQuery, PaginatedList<AdminPayoutDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminPayoutsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AdminPayoutDto>> Handle(ListAdminPayoutsQuery request, CancellationToken cancellationToken)
    {
        var payouts = _context.SellerPayouts
            .Join(_context.Sellers, p => p.SellerId, s => s.Id, (p, s) => new { Payout = p, s.BusinessName, s.Email });

        var waitingOnly = false;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = Enum.Parse<PayoutStatus>(request.Status, ignoreCase: true);
            payouts = payouts.Where(x => x.Payout.Status == status);
            waitingOnly = status == PayoutStatus.Requested;
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            payouts = payouts.Where(x => x.BusinessName.Contains(term) || x.Payout.AccountName.Contains(term));
        }

        var total = await payouts.CountAsync(cancellationToken);

        // Waiting requests: oldest first, so nobody waits too long. Otherwise newest first.
        var ordered = waitingOnly
            ? payouts.OrderBy(x => x.Payout.CreatedAt)
            : payouts.OrderByDescending(x => x.Payout.CreatedAt);

        var items = await ordered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AdminPayoutDto
            {
                Id = x.Payout.Id,
                Amount = x.Payout.Amount,
                Status = x.Payout.Status.ToString(),
                BankName = x.Payout.BankName,
                AccountNumber = x.Payout.AccountNumber,
                AccountName = x.Payout.AccountName,
                Reference = x.Payout.Reference,
                RejectReason = x.Payout.RejectReason,
                RequestedAt = x.Payout.CreatedAt,
                ProcessedAt = x.Payout.ProcessedAt,
                SellerId = x.Payout.SellerId,
                SellerName = x.BusinessName,
                SellerEmail = x.Email,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<AdminPayoutDto>(items, total, request.PageNumber, request.PageSize);
    }
}
