using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto?>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResultDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshAsync(request.RefreshToken, cancellationToken);

        // Don't throw here: throwing would roll back the transaction, and when a stolen
        // (already used) token is detected the "revoke all sessions" change must be saved.
        if (!result.Succeeded)
            return null;

        return new AuthResultDto
        {
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            UserId = result.UserId!.Value
        };
    }
}