using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.DeliveryFees;

public static class DeliveryFeeLookup
{
    // The fee for delivering to a state, or null if we don't deliver there.
    public static async Task<decimal?> GetDeliveryFeeAsync(
        this IApplicationDbContext context, string state, CancellationToken cancellationToken)
    {
        var name = state.Trim();
        return await context.DeliveryFees
            .Where(f => f.State == name)
            .Select(f => (decimal?)f.Amount)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
