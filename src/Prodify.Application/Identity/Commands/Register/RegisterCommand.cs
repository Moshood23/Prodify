using MediatR;

namespace Prodify.Application.Identity.Commands.Register;

public class RegisterCommand : IRequest<AuthResultDto>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}

public class AuthResultDto
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
}