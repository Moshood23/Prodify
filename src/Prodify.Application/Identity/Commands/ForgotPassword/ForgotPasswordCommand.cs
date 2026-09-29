using MediatR;

namespace Prodify.Application.Identity.Commands.ForgotPassword;

// Emails a password reset link. Always "succeeds", so nobody can use it to
// find out which email addresses have an account.
public class ForgotPasswordCommand : IRequest
{
    public string Email { get; set; } = null!;
}
