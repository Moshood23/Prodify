using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using ValidationException = Prodify.Application.Common.Exceptions.ValidationException;

namespace Prodify.Application.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private const string InvalidLinkMessage = "This reset link is invalid or has expired. Please request a new one.";

    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ResetPasswordCommandHandler(IIdentityService identityService, IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _identityService = identityService;
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var result = await _identityService.ResetPasswordAsync(email, request.Token, request.NewPassword, cancellationToken);

        if (!result.Succeeded)
        {
            // A bad link is a problem with the whole page; weak passwords belong under the password box.
            if (result.Errors.Contains(InvalidLinkMessage))
                throw new BusinessRuleException(InvalidLinkMessage);

            throw new ValidationException(result.Errors.Distinct().Select(e => new ValidationFailure(nameof(request.NewPassword), e)));
        }

        var firstName = await _context.Customers
            .Where(c => c.Email == email)
            .Select(c => c.FirstName)
            .FirstOrDefaultAsync(cancellationToken);

        // Tells the owner, in case it wasn't them.
        _emails.Enqueue(email, firstName, AccountEmails.PasswordChanged(firstName, _urls));
    }
}
