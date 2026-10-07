using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.Login;

// No transaction: a failed login must still save the wrong-password count.
public class LoginCommand : IRequest<AuthResultDto>, INoTransaction
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
