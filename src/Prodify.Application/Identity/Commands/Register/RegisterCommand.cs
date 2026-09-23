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
    // Short-lived JWT (15 minutes) sent as "Authorization: Bearer ..." on every request.
    public string Token { get; set; } = null!;

    // Long-lived, single-use token for POST /api/auth/refresh. Store it securely on the client.
    public string RefreshToken { get; set; } = null!;

    public Guid UserId { get; set; }
}