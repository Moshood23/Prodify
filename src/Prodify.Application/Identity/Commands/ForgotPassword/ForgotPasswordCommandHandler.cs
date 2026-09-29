using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ForgotPasswordCommandHandler(IIdentityService identityService, IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _identityService = identityService;
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var token = await _identityService.GeneratePasswordResetTokenAsync(email, cancellationToken);
        if (token is null)
            return;

        var firstName = await _context.Customers
            .Where(c => c.Email == email)
            .Select(c => c.FirstName)
            .FirstOrDefaultAsync(cancellationToken);

        var link = _urls.Page($"/reset-password?email={Uri.EscapeDataString(email)}&token={token}");
        _emails.Enqueue(email, firstName, AccountEmails.PasswordReset(firstName, link));
    }
}
