using Prodify.Domain.Common;

namespace Prodify.Domain.Cart.Entities;

public class Cart : AuditableEntity
{
    private readonly List<CartItem> _items = new();

    public Guid? CustomerId { get; private set; }
    public string? SessionId { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(i => i.Subtotal);

    private Cart()
    {
    }

    private Cart(Guid id, Guid? customerId, string? sessionId) : base(id)
    {
        CustomerId = customerId;
        SessionId = sessionId;
    }

    public static Cart CreateForCustomer(Guid customerId)
    {
        return new Cart(Guid.NewGuid(), customerId, null);
    }

    public static Cart CreateForGuest(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be empty for a guest cart.", nameof(sessionId));

        return new Cart(Guid.NewGuid(), null, sessionId);
    }

    public void AddItem(Guid productVariantId, int quantity, decimal unitPrice)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductVariantId == productVariantId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            return;
        }

        var item = CartItem.Create(Id, productVariantId, quantity, unitPrice);
        _items.Add(item);
    }

    public void UpdateItemQuantity(Guid cartItemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);

        if (item is null)
            throw new InvalidOperationException($"Cart item '{cartItemId}' not found.");

        item.UpdateQuantity(quantity);
    }

    public void RemoveItem(Guid cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);

        if (item is not null)
            _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void AssignToCustomer(Guid customerId)
    {
        CustomerId = customerId;
        SessionId = null;
    }
}