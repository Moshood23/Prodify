using Prodify.Domain.Common;

namespace Prodify.Domain.Payouts.Entities;

// What a seller earned from one delivered part of an order, at the commission
// rate on the day it was delivered. The money can be withdrawn from AvailableAt.
public class SellerEarning : Entity
{
    // Time for complaints or returns before the money can leave Prodify.
    public static readonly TimeSpan HoldPeriod = TimeSpan.FromDays(7);

    public Guid SellerId { get; private set; }
    public Guid SellerOrderId { get; private set; }
    public Guid OrderId { get; private set; }

    public decimal Sales { get; private set; }
    public decimal CommissionRate { get; private set; }
    public decimal Commission { get; private set; }
    public decimal NetAmount { get; private set; }

    public DateTime EarnedAt { get; private set; }
    public DateTime AvailableAt { get; private set; }

    private SellerEarning()
    {
    }

    private SellerEarning(Guid id) : base(id)
    {
    }

    public static SellerEarning Create(Guid sellerId, Guid sellerOrderId, Guid orderId, decimal sales, decimal commissionRate, DateTime earnedAt)
    {
        if (sales < 0)
            throw new ArgumentOutOfRangeException(nameof(sales), "Sales cannot be negative.");
        if (commissionRate is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(commissionRate), "Commission rate must be between 0 and 100.");

        var commission = Math.Round(sales * commissionRate / 100m, 2, MidpointRounding.AwayFromZero);

        return new SellerEarning(Guid.NewGuid())
        {
            SellerId = sellerId,
            SellerOrderId = sellerOrderId,
            OrderId = orderId,
            Sales = sales,
            CommissionRate = commissionRate,
            Commission = commission,
            NetAmount = sales - commission,
            EarnedAt = earnedAt,
            AvailableAt = earnedAt + HoldPeriod,
        };
    }
}
