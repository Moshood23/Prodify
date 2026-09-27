using MediatR;

namespace Prodify.Application.Admin.Customers.Queries.GetCustomerDetails;

// Admin: one customer's profile, addresses and order totals.
// Their orders come from GET /api/admin/orders?customerId=...
public class GetCustomerDetailsQuery : IRequest<CustomerDetailsDto>
{
    public Guid Id { get; set; }
}

public class CustomerDetailsDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public int OrderCount { get; set; }
    public int CancelledOrderCount { get; set; }
    public decimal TotalSpent { get; set; }

    public List<CustomerAddressSummaryDto> Addresses { get; set; } = new();
}

public class CustomerAddressSummaryDto
{
    public string Label { get; set; } = null!;
    public string RecipientName { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool IsDefault { get; set; }
}