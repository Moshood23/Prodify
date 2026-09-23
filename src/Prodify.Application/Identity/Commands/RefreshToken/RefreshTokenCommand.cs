using MediatR;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.RefreshToken;

// Returns null when the refresh token is invalid, expired or already used.
public class RefreshTokenCommand : IRequest<AuthResultDto?>
{
    public string RefreshToken { get; set; } = null!;
}