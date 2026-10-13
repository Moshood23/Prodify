using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Messaging.Common;
using Prodify.Domain.Messaging.Entities;

namespace Prodify.Application.Messaging.Commands;

// The customer or the seller adds a message to their conversation.
public class ReplyToConversationCommand : IRequest
{
    public Guid ConversationId { get; set; }
    // Set by the controller: which side is writing.
    public MessageSender As { get; set; }
    public string Message { get; set; } = null!;
}

public class ReplyToConversationCommandValidator : AbstractValidator<ReplyToConversationCommand>
{
    public ReplyToConversationCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty().WithMessage("Write a message.").MaximumLength(ConversationMessage.MaxLength);
    }
}

public class ReplyToConversationCommandHandler : IRequestHandler<ReplyToConversationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReplyToConversationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ReplyToConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.GetOwnConversationAsync(_currentUser, request.As, request.ConversationId, cancellationToken);

        _context.Add(conversation.Post(request.As, request.Message, DateTime.UtcNow));
        // Writing means they've seen the conversation.
        conversation.MarkReadBy(request.As);

        var name = request.As == MessageSender.Customer
            ? await _context.Customers.Where(c => c.Id == conversation.CustomerId).Select(c => c.FirstName).FirstAsync(cancellationToken)
            : await _context.Sellers.Where(s => s.Id == conversation.SellerId).Select(s => s.BusinessName).FirstAsync(cancellationToken);
        _context.NotifyIfFirstUnread(conversation, request.As, name);
    }
}

// Opening a conversation clears its unread count for that side.
public class MarkConversationReadCommand : IRequest
{
    public Guid ConversationId { get; set; }
    public MessageSender As { get; set; }
}

public class MarkConversationReadCommandHandler : IRequestHandler<MarkConversationReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkConversationReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.GetOwnConversationAsync(_currentUser, request.As, request.ConversationId, cancellationToken);
        conversation.MarkReadBy(request.As);
    }
}
