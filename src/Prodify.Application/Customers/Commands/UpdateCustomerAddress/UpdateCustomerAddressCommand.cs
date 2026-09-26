using MediatR;

namespace Prodify.Application.Customers.Commands.UpdateCustomerAddress;

public class UpdateCustomerAddressCommand : IRequest
{
    public Guid AddressId { get; set; }
    public string Label { get; set; } = null!;
    public string RecipientName { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}