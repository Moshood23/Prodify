using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Common;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Common;

// Emails and bell notifications as a return moves along.
public class ReturnEmailSender
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ReturnEmailSender(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task RequestedAsync(ReturnRequest request, string orderNumber, CancellationToken cancellationToken)
    {
        var message = $"A customer wants to return items from order {orderNumber} ({Describe(request.Reason)}). Please accept or reject it.";
        Notify(request.SellerId, "Return requested", message, "/seller/returns");

        var seller = await SellerAsync(request.SellerId, cancellationToken);
        if (seller is not null)
            _emails.Enqueue(seller.Value.Email, seller.Value.Name, EmailLayout.Build(
                $"Return requested for order {orderNumber}",
                "A customer wants to return items",
                new[]
                {
                    message,
                    request.Details is null ? "" : $"What they said: {request.Details}",
                    "The money for this order stays on hold until the return is settled.",
                }.Where(l => l.Length > 0).ToArray(),
                ("Open returns", _urls.Page("/seller/returns")),
                rows: ItemRows(request)));
    }

    public async Task RejectedAsync(ReturnRequest request, string orderNumber, CancellationToken cancellationToken)
    {
        var message = $"The seller didn't accept your return for order {orderNumber}: {request.SellerNote} You can ask Prodify to look at it.";
        Notify(request.CustomerId, "Return not accepted", message, $"/orders/{request.OrderId}");

        await EmailCustomerAsync(request, $"Your return for order {orderNumber} wasn't accepted", "Your return wasn't accepted",
            new[]
            {
                $"The seller didn't accept your return for order {orderNumber}.",
                $"Their reason: {request.SellerNote}",
                "If you don't agree, open your order and choose \"Ask Prodify to review\". We'll look at it and decide.",
            }, cancellationToken);
    }

    public async Task EscalatedAsync(ReturnRequest request, string orderNumber, CancellationToken cancellationToken)
    {
        Notify(request.SellerId, "Return sent to Prodify",
            $"The customer asked Prodify to review the return for order {orderNumber}. We'll let you know what we decide.", "/seller/returns");
        await Task.CompletedTask;
    }

    public async Task RefundedAsync(ReturnRequest request, string orderNumber, RefundSplit refund, CancellationToken cancellationToken)
    {
        var parts = new List<string>();
        if (refund.ToCard > 0)
            parts.Add($"{Naira.Format(refund.ToCard)} to your card");
        if (refund.ToCredit > 0)
            parts.Add($"{Naira.Format(refund.ToCredit)} as store credit");
        var where = parts.Count > 0 ? string.Join(" and ", parts) : Naira.Format(0);

        Notify(request.CustomerId, "Return accepted", $"Your return for order {orderNumber} was accepted. We've refunded {where}.", $"/orders/{request.OrderId}");
        Notify(request.SellerId, "Return refunded",
            $"The return for order {orderNumber} was refunded. {Naira.Format(request.ItemsValue)} came off your sales for that order.", "/seller/returns");

        var lines = new List<string> { $"Your return for order {orderNumber} was accepted." };
        if (refund.ToCard > 0)
            lines.Add($"We've refunded {Naira.Format(refund.ToCard)} to your card. Depending on your bank, it can take a few working days to show.");
        if (refund.ToCredit > 0)
            lines.Add($"{Naira.Format(refund.ToCredit)} went to your store credit, ready to use on your next order.");
        if (request.AdminNote is not null)
            lines.Add($"Note from Prodify: {request.AdminNote}");

        await EmailCustomerAsync(request, $"Return accepted for order {orderNumber}", "Your return was accepted", lines, cancellationToken);
    }

    public async Task DeclinedAsync(ReturnRequest request, string orderNumber, CancellationToken cancellationToken)
    {
        Notify(request.CustomerId, "Return declined", $"Prodify looked at your return for order {orderNumber} and declined it: {request.AdminNote}", $"/orders/{request.OrderId}");
        Notify(request.SellerId, "Return closed", $"Prodify declined the return for order {orderNumber}. Your money for it is no longer on hold.", "/seller/returns");

        await EmailCustomerAsync(request, $"Return declined for order {orderNumber}", "Your return was declined",
            new[]
            {
                $"We looked at your return for order {orderNumber} and can't accept it.",
                $"Reason: {request.AdminNote}",
            }, cancellationToken);
    }

    private async Task EmailCustomerAsync(ReturnRequest request, string subject, string heading, IReadOnlyCollection<string> lines, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == request.CustomerId)
            .Select(c => new { c.Email, c.FirstName })
            .FirstOrDefaultAsync(cancellationToken);
        if (customer is null)
            return;

        _emails.Enqueue(customer.Email, customer.FirstName, EmailLayout.Build(
            subject,
            heading,
            new[] { $"Hi {customer.FirstName}," }.Concat(lines).ToArray(),
            ("View your order", _urls.Page($"/orders/{request.OrderId}")),
            rows: ItemRows(request)));
    }

    private async Task<(string Email, string Name)?> SellerAsync(Guid sellerId, CancellationToken cancellationToken)
    {
        var seller = await _context.Sellers
            .Where(s => s.Id == sellerId)
            .Select(s => new { s.Email, s.BusinessName })
            .FirstOrDefaultAsync(cancellationToken);
        return seller is null ? null : (seller.Email, seller.BusinessName);
    }

    private void Notify(Guid recipientId, string title, string message, string link) =>
        _context.Add(Notification.Create(recipientId, NotificationType.Return, title, message, link));

    private static IEnumerable<(string Label, string Value, bool Bold)> ItemRows(ReturnRequest request) =>
        request.Items.Select(i => ($"{i.ProductName} x {i.Quantity}", Naira.Format(i.UnitPrice * i.Quantity), false));

    public static string Describe(ReturnReason reason) => reason switch
    {
        ReturnReason.Damaged => "damaged or faulty",
        ReturnReason.WrongItem => "wrong item sent",
        ReturnReason.NotAsDescribed => "not as described",
        ReturnReason.NoLongerNeeded => "no longer needed",
        _ => "other reason"
    };
}
