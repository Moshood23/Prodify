using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Common;
using Prodify.Application.Ordering.SellerOrders.Common;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Common.Emails;

// Queues the emails about an order, and the matching notifications on the website's bell:
// to the customer at each step, and to sellers when there's something for them to do (or to stop doing).
public class OrderEmailSender
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public OrderEmailSender(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    // Pay on delivery: when it's placed. Card: when the payment goes through.
    // Needs the order's Items and SellerOrders.
    public async Task OrderConfirmedAsync(Order order, CancellationToken cancellationToken)
    {
        var number = order.OrderNumber.Value;
        var summary = PaymentSummary(order);
        Notify(order.CustomerId,
            order.PaymentMethod == PaymentMethod.Card ? NotificationType.PaymentSuccessful : NotificationType.OrderPlaced,
            order.PaymentMethod == PaymentMethod.Card ? "Payment received" : "Order placed",
            summary,
            $"/orders/{order.Id}");

        var customer = await CustomerAsync(order, cancellationToken);
        if (customer is not null)
        {
            var paidByCard = order.PaymentMethod == PaymentMethod.Card;
            var address = order.ShippingAddress;

            _emails.Enqueue(customer.Value.Email, customer.Value.FirstName, EmailLayout.Build(
                paidByCard ? $"Payment received for order {order.OrderNumber.Value}" : $"We've received your order {order.OrderNumber.Value}",
                paidByCard ? "Thanks, your payment went through" : "Thanks for your order",
                new[]
                {
                    $"Hi {customer.Value.FirstName},",
                    summary,
                    $"Delivering to: {address.RecipientName}, {address.AddressLine1}, {address.City}, {address.State}.",
                },
                ("View your order", _urls.Page($"/orders/{order.Id}")),
                "We'll email you again when your items are on their way.",
                ItemRows(order.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice.Amount)))
                    .Append(("Delivery", order.DeliveryFee == 0 ? "Free" : Naira.Format(order.DeliveryFee), false))
                    .Concat(order.Discount > 0
                        ? new[] { ($"Voucher {order.VoucherCode}", "-" + Naira.Format(order.Discount), false) }
                        : Array.Empty<(string, string, bool)>())
                    .Concat(order.CreditUsed > 0
                        ? new[] { ("Store credit", "-" + Naira.Format(order.CreditUsed), false) }
                        : Array.Empty<(string, string, bool)>())
                    .Append(("Total", Naira.Format(order.Total.Amount), true))));
        }

        // Each seller hears about their own part.
        var sellers = await SellersAsync(order.SellerOrders.Select(so => so.SellerId), cancellationToken);
        foreach (var part in order.SellerOrders.Where(so => so.Status != SellerOrderStatus.Cancelled))
        {
            if (!sellers.TryGetValue(part.SellerId, out var seller))
                continue;

            var items = order.Items.Where(i => i.SellerOrderId == part.Id).ToList();
            Notify(part.SellerId, NotificationType.NewOrder, "New order to prepare",
                $"Order {number}: {items.Sum(i => i.Quantity)} item(s). Please confirm and pack it.",
                $"/seller/orders/{part.Id}");

            _emails.Enqueue(seller.Email, seller.BusinessName, EmailLayout.Build(
                $"New order {order.OrderNumber.Value} to prepare",
                "You have a new order",
                new[]
                {
                    $"Order {order.OrderNumber.Value} has {items.Sum(i => i.Quantity)} item(s) from {seller.BusinessName}.",
                    order.PaymentMethod == PaymentMethod.Card
                        ? "The customer has paid. Please confirm and pack it."
                        : "The customer will pay on delivery. Please confirm and pack it.",
                },
                ("Open the order", _urls.Page($"/seller/orders/{part.Id}")),
                rows: ItemRows(items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice.Amount)))));
        }
    }

    // One seller's part has left. Needs the part's Items.
    public async Task PartShippedAsync(Order order, SellerOrder part, string carrier, string? trackingNumber, CancellationToken cancellationToken)
    {
        var tracking = string.IsNullOrWhiteSpace(trackingNumber) ? "" : $", tracking number {trackingNumber.Trim()}";
        Notify(order.CustomerId, NotificationType.OrderShipped, "Your items are on their way",
            $"Items from order {order.OrderNumber.Value} were sent with {carrier.Trim()}{tracking}.",
            $"/orders/{order.Id}");

        var customer = await CustomerAsync(order, cancellationToken);
        if (customer is null)
            return;

        _emails.Enqueue(customer.Value.Email, customer.Value.FirstName, EmailLayout.Build(
            $"Your order {order.OrderNumber.Value} is on its way",
            "Your items are on their way",
            new[]
            {
                $"Hi {customer.Value.FirstName},",
                $"These items from order {order.OrderNumber.Value} were sent with {carrier.Trim()}{tracking}.",
                order.PaymentMethod == PaymentMethod.PayOnDelivery && !order.IsPaid
                    ? "Please have the payment ready when they arrive."
                    : "You don't need to pay anything when they arrive.",
            },
            ("Track your order", _urls.Page($"/orders/{order.Id}")),
            rows: ItemRows(part.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice.Amount)))));
    }

    // One seller's part has arrived. Needs the part's Items.
    public async Task PartDeliveredAsync(Order order, SellerOrder part, CancellationToken cancellationToken)
    {
        Notify(order.CustomerId, NotificationType.OrderDelivered, "Items delivered",
            $"Items from order {order.OrderNumber.Value} have been delivered. We hope you enjoy them!",
            $"/orders/{order.Id}");

        var customer = await CustomerAsync(order, cancellationToken);
        if (customer is null)
            return;

        _emails.Enqueue(customer.Value.Email, customer.Value.FirstName, EmailLayout.Build(
            $"Delivered: order {order.OrderNumber.Value}",
            "Your items were delivered",
            new[]
            {
                $"Hi {customer.Value.FirstName},",
                $"These items from order {order.OrderNumber.Value} have been delivered. We hope you enjoy them!",
                "If something is wrong with your order, reply to this email and we'll help.",
            },
            ("View your order", _urls.Page($"/orders/{order.Id}")),
            rows: ItemRows(part.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice.Amount)))));
    }

    // One line saying what was paid, or what is still to pay on delivery.
    private static string PaymentSummary(Order order)
    {
        var number = order.OrderNumber.Value;
        var credit = order.CreditUsed > 0 ? $" Store credit paid {Naira.Format(order.CreditUsed)}." : "";

        if (order.PaymentMethod == PaymentMethod.Card)
            return order.Total.Amount == 0
                ? $"Your store credit paid for order {number}. The seller will now prepare it."
                : $"We've received {Naira.Format(order.Total.Amount)} for order {number}.{credit} The seller will now prepare it.";

        return order.Total.Amount == 0
            ? $"Order {number} is placed and your store credit covers it, so there's nothing to pay when it arrives."
            : $"Order {number} is placed. You'll pay {Naira.Format(order.Total.Amount)} when it arrives.{credit}";
    }

    // Some or all of an order was cancelled. Needs the parts' Items.
    // refund: what went back to the customer's card and to their store credit.
    // tellSellers: also tell the sellers of these parts to stop, if they had been asked to prepare them.
    public async Task PartsCancelledAsync(
        Order order,
        IReadOnlyCollection<SellerOrder> parts,
        string reason,
        RefundSplit refund,
        bool tellSellers,
        CancellationToken cancellationToken)
    {
        var customer = await CustomerAsync(order, cancellationToken);
        var wholeOrder = order.SellerOrders.All(so => parts.Contains(so) || so.Status == SellerOrderStatus.Cancelled);
        var items = parts.SelectMany(p => p.Items).ToList();

        Notify(order.CustomerId, NotificationType.OrderCancelled,
            wholeOrder ? "Order cancelled" : "Items cancelled",
            (wholeOrder
                ? $"Order {order.OrderNumber.Value} was cancelled."
                : $"Some items in order {order.OrderNumber.Value} were cancelled.")
                + (refund.ToCard > 0 ? $" {Naira.Format(refund.ToCard)} was refunded to your card." : "")
                + (refund.ToCredit > 0 ? $" {Naira.Format(refund.ToCredit)} went back to your store credit." : "")
                + $" Reason: {reason}",
            $"/orders/{order.Id}");

        if (customer is not null)
        {
            var lines = new List<string>
            {
                $"Hi {customer.Value.FirstName},",
                wholeOrder
                    ? $"Your order {order.OrderNumber.Value} has been cancelled."
                    : $"Some items in your order {order.OrderNumber.Value} have been cancelled. The rest of your order is still coming.",
                $"Reason: {reason}",
            };
            if (refund.ToCard > 0)
                lines.Add($"We've refunded {Naira.Format(refund.ToCard)} to your card. Depending on your bank, it can take a few working days to show.");
            if (refund.ToCredit > 0)
                lines.Add($"{Naira.Format(refund.ToCredit)} went back to your store credit, ready to use on your next order.");

            _emails.Enqueue(customer.Value.Email, customer.Value.FirstName, EmailLayout.Build(
                wholeOrder ? $"Order {order.OrderNumber.Value} cancelled" : $"Items cancelled from order {order.OrderNumber.Value}",
                wholeOrder ? "Your order was cancelled" : "Some items were cancelled",
                lines,
                ("View your order", _urls.Page($"/orders/{order.Id}")),
                rows: ItemRows(items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice.Amount)))));
        }

        // Sellers only hear about orders they were asked to prepare.
        if (!tellSellers || !SellerOrderRules.IsReadyForSeller(order))
            return;

        var sellers = await SellersAsync(parts.Select(p => p.SellerId), cancellationToken);
        foreach (var part in parts)
        {
            Notify(part.SellerId, NotificationType.OrderCancelled, "Order cancelled",
                $"Order {order.OrderNumber.Value} was cancelled, so please don't send it. Reason: {reason}",
                $"/seller/orders/{part.Id}");

            if (!sellers.TryGetValue(part.SellerId, out var seller))
                continue;

            _emails.Enqueue(seller.Email, seller.BusinessName, EmailLayout.Build(
                $"Order {order.OrderNumber.Value} was cancelled",
                "An order was cancelled",
                new[]
                {
                    $"Order {order.OrderNumber.Value} was cancelled, so please don't send it.",
                    $"Reason: {reason}",
                    "The stock has been put back.",
                },
                ("Open the order", _urls.Page($"/seller/orders/{part.Id}")),
                rows: ItemRows(part.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice.Amount)))));
        }
    }

    private void Notify(Guid recipientId, NotificationType type, string title, string message, string link) =>
        _context.Add(Notification.Create(recipientId, type, title, message, link));

    private static IEnumerable<(string Label, string Value, bool Bold)> ItemRows(IEnumerable<(string Name, int Quantity, decimal UnitPrice)> items) =>
        items.Select(i => ($"{i.Name} x {i.Quantity}", Naira.Format(i.UnitPrice * i.Quantity), false));

    private async Task<(string Email, string FirstName)?> CustomerAsync(Order order, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == order.CustomerId)
            .Select(c => new { c.Email, c.FirstName })
            .FirstOrDefaultAsync(cancellationToken);

        return customer is null ? null : (customer.Email, customer.FirstName);
    }

    private async Task<Dictionary<Guid, (string Email, string BusinessName)>> SellersAsync(IEnumerable<Guid> sellerIds, CancellationToken cancellationToken)
    {
        var ids = sellerIds.Distinct().ToList();
        var sellers = await _context.Sellers
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Email, s.BusinessName })
            .ToListAsync(cancellationToken);

        return sellers.ToDictionary(s => s.Id, s => (s.Email, s.BusinessName));
    }
}
