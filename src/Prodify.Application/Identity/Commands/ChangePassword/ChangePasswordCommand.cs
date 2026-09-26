using MediatR;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.ChangePassword;

// Returns new tokens: every other session is logged out when the password changes.
public class ChangePasswordCommand : IRequest<AuthResultDto>
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}