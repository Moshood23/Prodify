using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Identity.Commands.VerifyEmail;

public static class EmailVerification
{
    public const string InvalidLink = "This link is invalid or has expired. Log in and ask for a new one.";
    public const string Required = "Please verify your email address first. We sent you a link when you signed up; you can ask for a new one at the top of the page.";

    // The website page the verification email links to.
    public static string Link(IAppUrls urls, Guid userId, string token) =>
        urls.Page($"/verify-email?user={userId}&token={token}");

    // Stops unverified accounts from placing orders or opening a store.
    public static async Task EnsureVerifiedAsync(this IIdentityService identity, ICurrentUserService currentUser, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new ForbiddenAccessException("You must be logged in.");
        if (!await identity.IsEmailVerifiedAsync(userId, cancellationToken))
            throw new BusinessRuleException(Required);
    }
}

// The link in the email. Works without being logged in (the email may be opened on another device).
public class VerifyEmailCommand : IRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(EmailVerification.InvalidLink);
        RuleFor(x => x.Token).NotEmpty().WithMessage(EmailVerification.InvalidLink);
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IIdentityService _identityService;

    public VerifyEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!await _identityService.VerifyEmailAsync(request.UserId, request.Token, cancellationToken))
            throw new BusinessRuleException(EmailVerification.InvalidLink);
    }
}

// "Send the link again", for the signed-in user.
public class ResendVerificationEmailCommand : IRequest
{
}

public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ResendVerificationEmailCommandHandler(
        IIdentityService identityService, ICurrentUserService currentUser, IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _identityService = identityService;
        _currentUser = currentUser;
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenAccessException("You must be logged in.");

        var token = await _identityService.GenerateEmailVerificationTokenAsync(userId, cancellationToken)
            ?? throw new BusinessRuleException("Your email address is already verified.");

        // Every account starts as a customer account, with the same email.
        var customer = await _context.Customers
            .Where(c => c.Id == _currentUser.CustomerId)
            .Select(c => new { c.Email, c.FirstName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessRuleException("Your account has no email address to verify.");

        _emails.Enqueue(customer.Email, customer.FirstName, AccountEmails.VerifyEmail(customer.FirstName, EmailVerification.Link(_urls, userId, token)));
    }
}
