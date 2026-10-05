using Prodify.Domain.Common;

namespace Prodify.Domain.Payouts.Entities;

// Settings an admin can change while the shop is running. There is one row.
public class PlatformSettings : AuditableEntity
{
    public const decimal DefaultCommissionRate = 10m;

    // Percent of each sale Prodify keeps, e.g. 10 = 10%.
    public decimal CommissionRate { get; private set; }

    private PlatformSettings()
    {
    }

    private PlatformSettings(Guid id) : base(id)
    {
        CommissionRate = DefaultCommissionRate;
    }

    public static PlatformSettings CreateDefault() => new(Guid.NewGuid());

    public void SetCommissionRate(decimal rate)
    {
        if (rate is < 0 or > 50)
            throw new ArgumentOutOfRangeException(nameof(rate), "Commission must be between 0% and 50%.");

        CommissionRate = Math.Round(rate, 2);
    }
}
