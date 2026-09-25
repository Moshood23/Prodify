using MediatR;

namespace Prodify.Application.Admin.Queries.GetAdminDashboard;

public class GetAdminDashboardQuery : IRequest<AdminDashboardDto>
{
}

public class AdminDashboardDto
{
    public int PendingSellers { get; set; }
    public int ApprovedSellers { get; set; }
    public int SuspendedSellers { get; set; }
    public int RejectedSellers { get; set; }

    public int Customers { get; set; }
    public int ActiveProducts { get; set; }

    public int TotalOrders { get; set; }
    public int OrdersToday { get; set; }

    // Money actually received (paid orders).
    public decimal PaidSales { get; set; }

    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class RecentOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal Total { get; set; }
}