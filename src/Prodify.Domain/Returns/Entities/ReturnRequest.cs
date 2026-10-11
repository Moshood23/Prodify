using Prodify.Domain.Common;

namespace Prodify.Domain.Returns.Entities;

public enum ReturnReason
{
    Damaged,
    WrongItem,
    NotAsDescribed,
    NoLongerNeeded,
    Other
}

// Requested -> Refunded (seller accepts) or Rejected (seller says no, with a reason).
// Rejected -> Escalated (customer asks Prodify) -> Refunded or Declined (Prodify's final word).
// An admin can also step in on a Requested return.
public enum ReturnStatus
{
    Requested,
    Rejected,
    Escalated,
    Refunded,
    Declined
}

// A customer sending back some or all items of one delivered seller part of an order.
public class ReturnRequest : AuditableEntity
{
    public const int MaxDetailsLength = 1000;
    public const int MaxNoteLength = 500;

    private readonly List<ReturnItem> _items = new();

    public Guid OrderId { get; private set; }
    public Guid SellerOrderId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid CustomerId { get; private set; }

    public ReturnReason Reason { get; private set; }
    public string? Details { get; private set; }
    public ReturnStatus Status { get; private set; }

    public string? SellerNote { get; private set; }
    public string? CustomerNote { get; private set; }
    public string? AdminNote { get; private set; }

    // What the customer gets back: the items less their share of any voucher. Delivery is not refunded.
    public decimal RefundAmount { get; private set; }
    public decimal RefundedToCard { get; private set; }
    public decimal RefundedAsCredit { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public IReadOnlyCollection<ReturnItem> Items => _items;

    // The items' price before any voucher, which is what comes off the seller's sales.
    public decimal ItemsValue => _items.Sum(i => i.UnitPrice * i.Quantity);

    public bool IsOpen => Status is ReturnStatus.Requested or ReturnStatus.Escalated;

    private ReturnRequest()
    {
    }

    private ReturnRequest(Guid id) : base(id)
    {
    }

    public static ReturnRequest Create(
        Guid orderId,
        Guid sellerOrderId,
        Guid sellerId,
        Guid customerId,
        ReturnReason reason,
        string? details,
        IEnumerable<(Guid ProductVariantId, string ProductName, int Quantity, decimal UnitPrice)> items,
        decimal refundAmount)
    {
        var request = new ReturnRequest(Guid.NewGuid())
        {
            OrderId = orderId,
            SellerOrderId = sellerOrderId,
            SellerId = sellerId,
            CustomerId = customerId,
            Reason = reason,
            Details = Clean(details, MaxDetailsLength),
            Status = ReturnStatus.Requested,
            RefundAmount = refundAmount,
        };

        foreach (var item in items)
            request._items.Add(ReturnItem.Create(request.Id, item.ProductVariantId, item.ProductName, item.Quantity, item.UnitPrice));

        if (request._items.Count == 0)
            throw new ArgumentException("Choose at least one item to return.", nameof(items));
        if (refundAmount < 0 || refundAmount > request.ItemsValue)
            throw new ArgumentOutOfRangeException(nameof(refundAmount), "The refund can't be more than the items.");

        return request;
    }

    public void Reject(string note)
    {
        Ensure(Status == ReturnStatus.Requested, "rejected");
        SellerNote = Required(note);
        Status = ReturnStatus.Rejected;
    }

    public void Escalate(string? note)
    {
        Ensure(Status == ReturnStatus.Rejected, "sent to Prodify");
        CustomerNote = Clean(note, MaxNoteLength);
        Status = ReturnStatus.Escalated;
    }

    // Seller accepted, or an admin approved: the money has gone back.
    public void MarkRefunded(decimal toCard, decimal toCredit, string? adminNote = null)
    {
        Ensure(IsOpen, "refunded");
        if (toCard < 0 || toCredit < 0 || toCard + toCredit > RefundAmount)
            throw new ArgumentOutOfRangeException(nameof(toCard), "Can't refund more than the return is worth.");

        RefundedToCard = toCard;
        RefundedAsCredit = toCredit;
        AdminNote = Clean(adminNote, MaxNoteLength);
        Status = ReturnStatus.Refunded;
        ResolvedAt = DateTime.UtcNow;
    }

    // Prodify's final no.
    public void Decline(string note)
    {
        Ensure(IsOpen, "declined");
        AdminNote = Required(note);
        Status = ReturnStatus.Declined;
        ResolvedAt = DateTime.UtcNow;
    }

    private void Ensure(bool allowed, string action)
    {
        if (!allowed)
            throw new InvalidOperationException($"A return that is {Status} can't be {action}.");
    }

    private static string Required(string note) =>
        Clean(note, MaxNoteLength) ?? throw new ArgumentException("Give a reason.", nameof(note));

    private static string? Clean(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var trimmed = text.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Keep it under {maxLength} characters.", nameof(text));

        return trimmed;
    }
}

public class ReturnItem : Entity
{
    public Guid ReturnRequestId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private ReturnItem()
    {
    }

    private ReturnItem(Guid id) : base(id)
    {
    }

    internal static ReturnItem Create(Guid returnRequestId, Guid productVariantId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Return at least one of each item you choose.");

        return new ReturnItem(Guid.NewGuid())
        {
            ReturnRequestId = returnRequestId,
            ProductVariantId = productVariantId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
        };
    }
}
