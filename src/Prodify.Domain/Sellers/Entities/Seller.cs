using Prodify.Domain.Common;

namespace Prodify.Domain.Sellers.Entities;

public enum SellerStatus
{
    PendingVerification,
    Approved,
    Suspended,
    Rejected
}

public class Seller : AuditableEntity
{
    private readonly List<SellerAddress> _addresses = new();

    public string BusinessName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }

    // Shown to shoppers on the seller's store page.
    public string? Description { get; private set; }

    public SellerStatus Status { get; private set; }

    // Why the application was rejected or the store suspended; shown to the seller.
    public string? StatusReason { get; private set; }

    // When an admin last approved, rejected, suspended or reinstated the seller.
    public DateTime? StatusChangedAt { get; private set; }

    public ICollection<SellerAddress> Addresses => _addresses;

    private Seller()
    {
    }

    private Seller(Guid id, string businessName, string email, string? phoneNumber, string? description) : base(id)
    {
        BusinessName = businessName;
        Email = email;
        PhoneNumber = phoneNumber;
        Description = description;
        Status = SellerStatus.PendingVerification;
    }

    public static Seller Create(string businessName, string email, string? phoneNumber = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name cannot be empty.", nameof(businessName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        return new Seller(
            Guid.NewGuid(),
            businessName.Trim(),
            email.Trim().ToLowerInvariant(),
            phoneNumber?.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim());
    }

    public void UpdateProfile(string businessName, string? phoneNumber, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name cannot be empty.", nameof(businessName));

        BusinessName = businessName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Approve()
    {
        if (Status != SellerStatus.PendingVerification)
            throw new InvalidOperationException($"Cannot approve a seller with status '{Status}'.");

        ChangeStatus(SellerStatus.Approved, null);
    }

    public void Reject(string? reason = null)
    {
        if (Status != SellerStatus.PendingVerification)
            throw new InvalidOperationException($"Cannot reject a seller with status '{Status}'.");

        ChangeStatus(SellerStatus.Rejected, reason);
    }

    public void Suspend(string? reason = null)
    {
        if (Status != SellerStatus.Approved)
            throw new InvalidOperationException($"Cannot suspend a seller with status '{Status}'.");

        ChangeStatus(SellerStatus.Suspended, reason);
    }

    public void Reinstate()
    {
        if (Status != SellerStatus.Suspended)
            throw new InvalidOperationException($"Cannot reinstate a seller with status '{Status}'.");

        ChangeStatus(SellerStatus.Approved, null);
    }

    // A rejected seller can fix their details and apply again.
    public void Reapply(string businessName, string? phoneNumber, string? description)
    {
        if (Status != SellerStatus.Rejected)
            throw new InvalidOperationException($"Only rejected sellers can reapply (status is '{Status}').");

        UpdateProfile(businessName, phoneNumber, description);
        Status = SellerStatus.PendingVerification;
        StatusReason = null;
    }

    public SellerAddress AddAddress(
        string addressLine1,
        string city,
        string state,
        string country,
        string phoneNumber,
        string? addressLine2 = null,
        string? postalCode = null)
    {
        var address = SellerAddress.Create(Id, addressLine1, city, state, country, phoneNumber, addressLine2, postalCode);
        _addresses.Add(address);

        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);

        if (address is not null)
            _addresses.Remove(address);
    }

    private void ChangeStatus(SellerStatus status, string? reason)
    {
        Status = status;
        StatusReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        StatusChangedAt = DateTime.UtcNow;
    }
}