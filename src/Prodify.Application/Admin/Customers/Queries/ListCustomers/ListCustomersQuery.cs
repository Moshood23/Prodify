using MediatR;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Admin.Customers.Queries.ListCustomers;

public class ListCustomersQuery : IRequest<PaginatedList<CustomerSummaryDto>>
{
    // Name, email or phone number.
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }

    // Paid orders only.
    public decimal TotalSpent { get; set; }
}