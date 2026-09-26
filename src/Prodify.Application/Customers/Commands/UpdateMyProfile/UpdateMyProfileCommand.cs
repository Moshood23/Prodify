using MediatR;

namespace Prodify.Application.Customers.Commands.UpdateMyProfile;

public class UpdateMyProfileCommand : IRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}