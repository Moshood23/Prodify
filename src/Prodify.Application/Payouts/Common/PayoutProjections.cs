using System.Linq.Expressions;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Payouts.Common;

public static class PayoutProjections
{
    public static readonly Expression<Func<SellerPayout, PayoutDto>> ToDto = p => new PayoutDto
    {
        Id = p.Id,
        Amount = p.Amount,
        Status = p.Status.ToString(),
        BankName = p.BankName,
        AccountNumber = p.AccountNumber,
        AccountName = p.AccountName,
        Reference = p.Reference,
        RejectReason = p.RejectReason,
        RequestedAt = p.CreatedAt,
        ProcessedAt = p.ProcessedAt,
    };
}
