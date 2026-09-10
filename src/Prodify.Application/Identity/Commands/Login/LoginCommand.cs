using MediatR;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.Login;

public class LoginCommand : IRequest<AuthResultDto>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}