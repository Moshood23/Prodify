using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Returns.Common;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Commands;

// The customer disagrees with the seller and asks Prodify to decide.
public class EscalateReturnCommand : IRequest
{
    public Guid ReturnId { get; set; }
    public string? Note { get; set; }
}

public class EscalateReturnCommandValidator : AbstractValidator<EscalateReturnCommand>
{
    public EscalateReturnCommandValidator()
    {
        RuleFor(x => x.Note).MaximumLength(ReturnRequest.MaxNoteLength);
    }
}

public class EscalateReturnCommandHandler : IRequestHandler<EscalateReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ReturnEmailSender _emails;

    public EscalateReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, ReturnEmailSender emails)
    {
        _context = context;
        _currentUser = currentUser;
        _emails = emails;
    }

    public async Task Handle(EscalateReturnCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();
        var (returnRequest, orderNumber) = await _context.GetReturnAsync(request.ReturnId, r => r.CustomerId == customerId, cancellationToken);

        // The seller answered when the return was last changed.
        var answeredAt = returnRequest.ModifiedAt ?? returnRequest.CreatedAt;
        if (returnRequest.Status == ReturnStatus.Rejected && DateTime.UtcNow > answeredAt + ReturnRefunds.Window)
            throw new BusinessRuleException($"You can ask Prodify to review a return within {ReturnRefunds.Window.TotalDays:0} days of the seller's answer.");

        returnRequest.Escalate(request.Note);

        // The seller's money waits again until Prodify decides.
        var earning = await _context.SellerEarnings.FirstOrDefaultAsync(e => e.SellerOrderId == returnRequest.SellerOrderId, cancellationToken);
        earning?.HoldForReturn();

        await _emails.EscalatedAsync(returnRequest, orderNumber, cancellationToken);
    }
}
