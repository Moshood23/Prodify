using FluentValidation;
using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Returns.Common;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Commands;

// Prodify decides for the customer: the refund goes ahead, whatever the seller said.
public class ApproveReturnCommand : IRequest
{
    public Guid ReturnId { get; set; }
    public string? Note { get; set; }
}

public class ApproveReturnCommandValidator : AbstractValidator<ApproveReturnCommand>
{
    public ApproveReturnCommandValidator()
    {
        RuleFor(x => x.Note).MaximumLength(ReturnRequest.MaxNoteLength);
    }
}

public class ApproveReturnCommandHandler : IRequestHandler<ApproveReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly ReturnEmailSender _emails;

    public ApproveReturnCommandHandler(IApplicationDbContext context, IPaymentService paymentService, ReturnEmailSender emails)
    {
        _context = context;
        _paymentService = paymentService;
        _emails = emails;
    }

    public async Task Handle(ApproveReturnCommand request, CancellationToken cancellationToken)
    {
        var (returnRequest, orderNumber) = await _context.GetReturnAsync(request.ReturnId, _ => true, cancellationToken);

        var refund = await _context.RefundReturnAsync(_paymentService, returnRequest, request.Note, cancellationToken);
        await _emails.RefundedAsync(returnRequest, orderNumber, refund, cancellationToken);
    }
}

// Prodify's final no.
public class DeclineReturnCommand : IRequest
{
    public Guid ReturnId { get; set; }
    public string Note { get; set; } = null!;
}

public class DeclineReturnCommandValidator : AbstractValidator<DeclineReturnCommand>
{
    public DeclineReturnCommandValidator()
    {
        RuleFor(x => x.Note).NotEmpty().WithMessage("Tell the customer why.").MaximumLength(ReturnRequest.MaxNoteLength);
    }
}

public class DeclineReturnCommandHandler : IRequestHandler<DeclineReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ReturnEmailSender _emails;

    public DeclineReturnCommandHandler(IApplicationDbContext context, ReturnEmailSender emails)
    {
        _context = context;
        _emails = emails;
    }

    public async Task Handle(DeclineReturnCommand request, CancellationToken cancellationToken)
    {
        var (returnRequest, orderNumber) = await _context.GetReturnAsync(request.ReturnId, _ => true, cancellationToken);

        returnRequest.Decline(request.Note);
        await _context.ReleaseEarningAsync(returnRequest, cancellationToken);
        await _emails.DeclinedAsync(returnRequest, orderNumber, cancellationToken);
    }
}
