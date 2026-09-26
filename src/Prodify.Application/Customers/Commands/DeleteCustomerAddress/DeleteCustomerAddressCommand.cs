using MediatR;

namespace Prodify.Application.Customers.Commands.DeleteCustomerAddress;

public class DeleteCustomerAddressCommand : IRequest
{
    public Guid AddressId { get; set; }
}