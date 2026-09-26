using FluentValidation.Results;
using MediatR;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Identity.Commands.Register;

namespace Prodify.Application.Identity.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, AuthResultDto>
{
    private const string WrongPasswordMessage = "Your current password is incorrect.";

    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(IIdentityService identityService, ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<AuthResultDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be logged in to change your password.");

        var result = await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);

        if (!result.Succeeded)
        {
            // Show each problem under the field it belongs to.
            var failures = result.Errors.Select(error => new ValidationFailure(
                error == WrongPasswordMessage ? nameof(request.CurrentPassword) : nameof(request.NewPassword),
                error));

            throw new ValidationException(failures);
        }

        return new AuthResultDto
        {
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            UserId = result.UserId!.Value
        };
    }
}