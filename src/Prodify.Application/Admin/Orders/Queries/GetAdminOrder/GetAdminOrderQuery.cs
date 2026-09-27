using MediatR;
using Prodify.Application.Ordering.Queries.GetOrder;

namespace Prodify.Application.Admin.Orders.Queries.GetAdminOrder;

// The order as the customer sees it, plus who placed it and each seller's step history.
public class GetAdminOrderQuery : IRequest<AdminOrderDto>
{
    public Guid Id { get; set; }
}

public class AdminOrderDto
{
    public OrderDto Order { get; set; } = null!;
    public AdminOrderCustomerDto Customer { get; set; } = null!;
    public List<AdminSellerOrderProgressDto> SellerProgress { get; set; } = new();
}

public class AdminOrderCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}

public class AdminSellerOrderProgressDto
{
    public Guid SellerOrderId { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public List<AdminStatusHistoryDto> History { get; set; } = new();
}

public class AdminStatusHistoryDto
{
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime OccurredAt { get; set; }
}