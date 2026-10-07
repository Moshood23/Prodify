using MediatR;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (!result.Succeeded)
            // "Invalid email or password." or, after 5 wrong tries, the lockout message.
            throw new BusinessRuleException(result.Errors.FirstOrDefault() ?? "Invalid email or password.");

        return new AuthResultDto
        {
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            UserId = result.UserId!.Value
        };
    }
}
