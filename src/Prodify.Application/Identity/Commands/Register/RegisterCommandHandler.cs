using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Application.Identity.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public RegisterCommandHandler(IApplicationDbContext context, IIdentityService identityService, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _identityService = identityService;
        _emails = emails;
        _urls = urls;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _context.Customers.AnyAsync(c => c.Email == email, cancellationToken);

        if (emailTaken)
            throw new ConflictException($"An account with email '{email}' already exists.");

        var customer = Customer.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber);
        _context.Add(customer);

        await _context.SaveChangesAsync(cancellationToken);

        var result = await _identityService.RegisterAsync(
            request.Email, request.Password, customer.Id, null, Roles.Customer, cancellationToken);

        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join(" ", result.Errors));

        _emails.Enqueue(email, customer.FirstName, AccountEmails.Welcome(customer.FirstName, _urls));

        return new AuthResultDto
        {
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            UserId = result.UserId!.Value
        };
    }
}
