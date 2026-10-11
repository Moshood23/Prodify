using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Returns.Common;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Commands;

// A customer asks to send back items from one delivered part of their order.
public class RequestReturnCommand : IRequest<Guid>
{
    public Guid SellerOrderId { get; set; }

    // One of ReturnReason: Damaged, WrongItem, NotAsDescribed, NoLongerNeeded, Other.
    public string Reason { get; set; } = null!;
    public string? Details { get; set; }
    public List<ReturnItemInput> Items { get; set; } = new();
}

public class ReturnItemInput
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class RequestReturnCommandValidator : AbstractValidator<RequestReturnCommand>
{
    public RequestReturnCommandValidator()
    {
        RuleFor(x => x.SellerOrderId).NotEmpty();
        RuleFor(x => x.Reason)
            .Must(r => Enum.TryParse<ReturnReason>(r, ignoreCase: true, out _))
            .WithMessage("Choose why you're returning the items.");
        RuleFor(x => x.Details).MaximumLength(ReturnRequest.MaxDetailsLength);
        RuleFor(x => x.Details)
            .NotEmpty()
            .When(x => string.Equals(x.Reason, nameof(ReturnReason.Other), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Tell us what's wrong.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Choose at least one item to return.");
        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.ProductVariantId).Distinct().Count() == items.Count)
            .WithMessage("Each item can only be listed once.");
        RuleForEach(x => x.Items).ChildRules(item => item.RuleFor(i => i.Quantity).GreaterThan(0));
    }
}

public class RequestReturnCommandHandler : IRequestHandler<RequestReturnCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ReturnEmailSender _emails;

    public RequestReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, ReturnEmailSender emails)
    {
        _context = context;
        _currentUser = currentUser;
        _emails = emails;
    }

    public async Task<Guid> Handle(RequestReturnCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.Items)
            .FirstOrDefaultAsync(o => o.SellerOrders.Any(so => so.Id == request.SellerOrderId), cancellationToken);

        if (order is null || order.CustomerId != customerId)
            throw new NotFoundException("Order", request.SellerOrderId);

        var part = order.SellerOrders.First(so => so.Id == request.SellerOrderId);
        if (part.Status != SellerOrderStatus.Delivered)
            throw new BusinessRuleException("You can only return items once they've been delivered.");

        if (await _context.ReturnRequests.AnyAsync(r => r.SellerOrderId == part.Id, cancellationToken))
            throw new BusinessRuleException("You've already asked to return items from this delivery.");

        // The seller's money for the part is held for 7 days after delivery; returns close with it.
        var earning = await _context.SellerEarnings.FirstOrDefaultAsync(e => e.SellerOrderId == part.Id, cancellationToken);
        if (earning is null || DateTime.UtcNow >= earning.AvailableAt)
            throw new BusinessRuleException($"Returns close {ReturnRefunds.Window.TotalDays:0} days after delivery.");

        var items = new List<(Guid, string, int, decimal)>();
        foreach (var wanted in request.Items)
        {
            var bought = part.Items.FirstOrDefault(i => i.ProductVariantId == wanted.ProductVariantId)
                ?? throw new BusinessRuleException("One of those items isn't in this delivery.");
            if (wanted.Quantity > bought.Quantity)
                throw new BusinessRuleException($"You can return at most {bought.Quantity} of {bought.ProductName}.");

            items.Add((bought.ProductVariantId, bought.ProductName, wanted.Quantity, bought.UnitPrice.Amount));
        }

        // The customer gets back what they paid for the items: their price less their share of any voucher.
        var itemsValue = items.Sum(i => i.Item3 * i.Item4);
        var refund = itemsValue - order.DiscountShareOf(itemsValue);

        var returnRequest = ReturnRequest.Create(
            order.Id, part.Id, part.SellerId, customerId,
            Enum.Parse<ReturnReason>(request.Reason, ignoreCase: true), request.Details, items, refund);
        _context.Add(returnRequest);
        earning.HoldForReturn();

        await _emails.RequestedAsync(returnRequest, order.OrderNumber.Value, cancellationToken);
        return returnRequest.Id;
    }
}
