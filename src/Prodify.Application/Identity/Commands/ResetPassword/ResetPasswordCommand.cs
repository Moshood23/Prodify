using MediatR;

namespace Prodify.Application.Identity.Commands.ResetPassword;

// Sets a new password with the token from the reset email.
public class ResetPasswordCommand : IRequest
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
