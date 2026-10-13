using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Messaging.Common;
using Prodify.Domain.Messaging.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Messaging.Commands;

// A customer writes to a seller about a product ("Ask the seller") or about an order they placed.
// If they already have a conversation about it, the message goes there. Returns the conversation's id.
public class StartConversationCommand : IRequest<Guid>
{
    public Guid? ProductId { get; set; }
    public Guid? OrderId { get; set; }
    // For an order with items from several sellers: which one.
    public Guid? SellerId { get; set; }
    public string Message { get; set; } = null!;
}

public class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x).Must(x => x.ProductId.HasValue != x.OrderId.HasValue)
            .WithName("Topic").WithMessage("Say which product or which order the message is about.");
        RuleFor(x => x.SellerId).NotEmpty().When(x => x.OrderId.HasValue).WithMessage("Say which seller to write to.");
        RuleFor(x => x.Message).NotEmpty().WithMessage("Write a message.").MaximumLength(ConversationMessage.MaxLength);
    }
}

public class StartConversationCommandHandler : IRequestHandler<StartConversationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public StartConversationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();
        var (sellerId, subject) = request.ProductId is { } productId
            ? await AboutProductAsync(productId, cancellationToken)
            : await AboutOrderAsync(customerId, request.OrderId!.Value, request.SellerId!.Value, cancellationToken);

        if (_currentUser.SellerId == sellerId)
            throw new BusinessRuleException("This is your own store.");

        var conversation = await _context.Conversations.FirstOrDefaultAsync(c =>
            c.CustomerId == customerId && c.SellerId == sellerId && c.ProductId == request.ProductId && c.OrderId == request.OrderId,
            cancellationToken);

        if (conversation is null)
        {
            var sellerApproved = await _context.Sellers.AnyAsync(s => s.Id == sellerId && s.Status == SellerStatus.Approved, cancellationToken);
            if (!sellerApproved)
                throw new BusinessRuleException("This store isn't taking messages right now.");

            conversation = Conversation.Start(customerId, sellerId, request.ProductId, request.OrderId, subject);
            _context.Add(conversation);
        }

        _context.Add(conversation.Post(MessageSender.Customer, request.Message, DateTime.UtcNow));

        var name = await _context.Customers.Where(c => c.Id == customerId).Select(c => c.FirstName).FirstAsync(cancellationToken);
        _context.NotifyIfFirstUnread(conversation, MessageSender.Customer, name);

        return conversation.Id;
    }

    private async Task<(Guid SellerId, string Subject)> AboutProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId && p.IsActive)
            .Select(p => new { p.SellerId, p.Name })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        return (product.SellerId, product.Name);
    }

    private async Task<(Guid SellerId, string Subject)> AboutOrderAsync(Guid customerId, Guid orderId, Guid sellerId, CancellationToken cancellationToken)
    {
        var orderNumber = await _context.Orders
            .Where(o => o.Id == orderId && o.CustomerId == customerId && o.SellerOrders.Any(so => so.SellerId == sellerId))
            .Select(o => o.OrderNumber.Value)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Order", orderId);

        return (sellerId, $"Order {orderNumber}");
    }
}
