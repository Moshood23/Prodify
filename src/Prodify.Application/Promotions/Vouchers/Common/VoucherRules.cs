using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.Vouchers.Common;

public static class VoucherRules
{
    // The voucher behind a code the customer typed, and what it takes off items worth itemsTotal.
    // Throws a message the customer can read if the code can't be used.
    public static async Task<(Voucher Voucher, decimal Discount)> GetUsableVoucherAsync(
        this IApplicationDbContext context, string code, decimal itemsTotal, CancellationToken cancellationToken)
    {
        var normalized = Voucher.Normalize(code);
        var voucher = await context.Vouchers.FirstOrDefaultAsync(v => v.Code == normalized, cancellationToken)
            ?? throw new BusinessRuleException($"We don't recognise the voucher code {normalized}.");

        if (!voucher.IsActive)
            throw new BusinessRuleException($"The voucher {voucher.Code} has ended.");

        if (itemsTotal < voucher.MinOrderAmount)
            throw new BusinessRuleException(
                $"The voucher {voucher.Code} needs items worth at least {Naira.Format(voucher.MinOrderAmount)}. Delivery doesn't count.");

        return (voucher, voucher.DiscountFor(itemsTotal));
    }
}

// Admin list and edit form.
public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    // "Percent" or "Fixed".
    public string DiscountType { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public bool IsActive { get; set; }
    public int TimesUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}
