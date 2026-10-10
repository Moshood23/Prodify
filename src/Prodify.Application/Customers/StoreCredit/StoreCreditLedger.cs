using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Customers.StoreCredit;

public static class StoreCreditLedger
{
    public static async Task<decimal> GetStoreCreditBalanceAsync(
        this IApplicationDbContext context, Guid customerId, CancellationToken cancellationToken) =>
        await context.StoreCreditEntries
            .Where(e => e.CustomerId == customerId)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0;
}
