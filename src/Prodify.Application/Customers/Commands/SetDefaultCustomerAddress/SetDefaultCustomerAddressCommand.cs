using MediatR;

namespace Prodify.Application.Customers.Commands.SetDefaultCustomerAddress;

public class SetDefaultCustomerAddressCommand : IRequest
{
    public Guid AddressId { get; set; }
}