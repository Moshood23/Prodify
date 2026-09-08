using MediatR;

namespace Prodify.Application.Customers.Commands.RegisterCustomer;

public class RegisterCustomerCommand : IRequest<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}