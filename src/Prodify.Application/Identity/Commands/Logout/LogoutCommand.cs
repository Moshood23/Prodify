using MediatR;

namespace Prodify.Application.Identity.Commands.Logout;

public class LogoutCommand : IRequest
{
    public string RefreshToken { get; set; } = null!;
}