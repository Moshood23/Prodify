using MediatR;

namespace Prodify.Application.Customers.Queries.GetCustomer;

public class GetCustomerQuery : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public List<CustomerAddressDto> Addresses { get; set; } = new();
}

public class CustomerAddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public string RecipientName { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool IsDefault { get; set; }
}