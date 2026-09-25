namespace Prodify.Application.Sellers.Common;

public class SellerAddressDto
{
    public Guid Id { get; set; }
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}

// The seller's own view of their store (Seller Centre) and part of the admin view.
public class SellerProfileDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public string? StatusReason { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public SellerAddressDto? PickupAddress { get; set; }
}