using MediatR;

namespace Prodify.Application.Ordering.Commands.PlaceOrder;

public class PlaceOrderCommand : IRequest<Guid>
{
    public Guid CustomerId { get; set; }
    public string RecipientName { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}