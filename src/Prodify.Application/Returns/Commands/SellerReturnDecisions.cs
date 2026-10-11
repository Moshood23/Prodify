using FluentValidation;
using MediatR;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Returns.Common;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Commands;

// The seller agrees: the customer is refunded straight away.
public class AcceptReturnCommand : IRequest
{
    public Guid ReturnId { get; set; }
}

public class AcceptReturnCommandHandler : IRequestHandler<AcceptReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly ReturnEmailSender _emails;

    public AcceptReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPaymentService paymentService, ReturnEmailSender emails)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _emails = emails;
    }

    public async Task Handle(AcceptReturnCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();
        var (returnRequest, orderNumber) = await _context.GetReturnAsync(request.ReturnId, r => r.SellerId == sellerId, cancellationToken);

        if (returnRequest.Status != ReturnStatus.Requested)
            throw new BusinessRuleException("Only a new return can be accepted.");

        var refund = await _context.RefundReturnAsync(_paymentService, returnRequest, null, cancellationToken);
        await _emails.RefundedAsync(returnRequest, orderNumber, refund, cancellationToken);
    }
}

// The seller says no, with a reason. The customer can then ask Prodify to review it.
public class RejectReturnCommand : IRequest
{
    public Guid ReturnId { get; set; }
    public string Note { get; set; } = null!;
}

public class RejectReturnCommandValidator : AbstractValidator<RejectReturnCommand>
{
    public RejectReturnCommandValidator()
    {
        RuleFor(x => x.Note).NotEmpty().WithMessage("Tell the customer why.").MaximumLength(ReturnRequest.MaxNoteLength);
    }
}

public class RejectReturnCommandHandler : IRequestHandler<RejectReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ReturnEmailSender _emails;

    public RejectReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, ReturnEmailSender emails)
    {
        _context = context;
        _currentUser = currentUser;
        _emails = emails;
    }

    public async Task Handle(RejectReturnCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();
        var (returnRequest, orderNumber) = await _context.GetReturnAsync(request.ReturnId, r => r.SellerId == sellerId, cancellationToken);

        returnRequest.Reject(request.Note);
        await _context.ReleaseEarningAsync(returnRequest, cancellationToken);
        await _emails.RejectedAsync(returnRequest, orderNumber, cancellationToken);
    }
}
