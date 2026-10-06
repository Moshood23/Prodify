using MediatR;

namespace Prodify.Application.Reports.Queries.GetSalesChart;

// Sales per day for the last 7, 30 or 90 days: one seller's, or the whole shop's.
public class GetSalesChartQuery : IRequest<SalesChartDto>
{
    public int Days { get; set; } = 30;

    // Set by the controller: true for a seller's own sales, false for the whole shop (admins).
    public bool OwnSalesOnly { get; set; }
}

public class SalesChartDto
{
    public int Days { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }

    // Whole shop only: Prodify's commission on orders delivered in the period.
    public decimal? CommissionEarned { get; set; }

    // One entry per day, oldest first, including days with no sales.
    public List<SalesDayDto> Points { get; set; } = new();
}

public class SalesDayDto
{
    // "2026-10-05", the day in Nigeria (WAT).
    public string Date { get; set; } = null!;
    public decimal Sales { get; set; }
    public int Orders { get; set; }
}
