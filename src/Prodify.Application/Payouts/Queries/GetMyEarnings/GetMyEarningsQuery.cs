using MediatR;
using Prodify.Application.Payouts.Common;

namespace Prodify.Application.Payouts.Queries.GetMyEarnings;

// The seller's Earnings page: balances, bank account, recent earnings and payouts.
public class GetMyEarningsQuery : IRequest<EarningsDto>
{
}

public class EarningsDto
{
    public decimal Available { get; set; }
    public decimal OnHold { get; set; }
    public decimal InProgress { get; set; }
    public decimal PaidOut { get; set; }
    public decimal TotalEarned { get; set; }

    // The rules, so the page can explain them.
    public decimal CommissionRate { get; set; }
    public int HoldDays { get; set; }
    public decimal MinimumPayout { get; set; }

    public PayoutAccountDto? Account { get; set; }
    public bool HasOpenRequest { get; set; }

    public List<EarningDto> RecentEarnings { get; set; } = new();
    public List<PayoutDto> Payouts { get; set; } = new();
}

public class EarningDto
{
    public Guid OrderId { get; set; }
    public Guid SellerOrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal Sales { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal Commission { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime EarnedAt { get; set; }
    public DateTime AvailableAt { get; set; }

    // Past the hold, so it counts towards what can be withdrawn.
    public bool IsAvailable { get; set; }
}
