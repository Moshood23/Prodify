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
    public SellerStatus Status { get; private set; }

    public IReadOnlyCollection<SellerAddress> Addresses => _addresses.AsReadOnly();

    private Seller()
    {
    }

    private Seller(Guid id, string businessName, string email, string? phoneNumber) : base(id)
    {
        BusinessName = businessName;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = SellerStatus.PendingVerification;
    }

    public static Seller Create(string businessName, string email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name cannot be empty.", nameof(businessName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        return new Seller(Guid.NewGuid(), businessName.Trim(), email.Trim().ToLowerInvariant(), phoneNumber?.Trim());
    }

    public void UpdateProfile(string businessName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name cannot be empty.", nameof(businessName));

        BusinessName = businessName.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }

    public void Approve()
    {
        if (Status != SellerStatus.PendingVerification)
            throw new InvalidOperationException($"Cannot approve a seller with status '{Status}'.");

        Status = SellerStatus.Approved;
    }

    public void Reject()
    {
        if (Status != SellerStatus.PendingVerification)
            throw new InvalidOperationException($"Cannot reject a seller with status '{Status}'.");

        Status = SellerStatus.Rejected;
    }

    public void Suspend()
    {
        if (Status != SellerStatus.Approved)
            throw new InvalidOperationException($"Cannot suspend a seller with status '{Status}'.");

        Status = SellerStatus.Suspended;
    }

    public void Reinstate()
    {
        if (Status != SellerStatus.Suspended)
            throw new InvalidOperationException($"Cannot reinstate a seller with status '{Status}'.");

        Status = SellerStatus.Approved;
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
}