using Prodify.Domain.Common;

namespace Prodify.Domain.Customers.Entities;

public class Customer : AuditableEntity
{
    private readonly List<CustomerAddress> _addresses = new();

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    private Customer()
    {
    }

    private Customer(Guid id, string firstName, string lastName, string email, string? phoneNumber) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        IsActive = true;
    }

    public static Customer Create(string firstName, string lastName, string email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        return new Customer(Guid.NewGuid(), firstName.Trim(), lastName.Trim(), email.Trim().ToLowerInvariant(), phoneNumber?.Trim());
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }

    public CustomerAddress AddAddress(
        string label,
        string recipientName,
        string addressLine1,
        string city,
        string state,
        string country,
        string phoneNumber,
        string? addressLine2 = null,
        string? postalCode = null,
        bool setAsDefault = false)
    {
        var isFirstAddress = !_addresses.Any();
        var address = CustomerAddress.Create(
            Id, label, recipientName, addressLine1, city, state, country, phoneNumber,
            addressLine2, postalCode, setAsDefault || isFirstAddress);

        if (address.IsDefault)
            ClearOtherDefaults();

        _addresses.Add(address);

        return address;
    }

    public void SetDefaultAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);

        if (address is null)
            throw new InvalidOperationException($"Address '{addressId}' not found.");

        ClearOtherDefaults();
        address.SetAsDefault();
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);

        if (address is not null)
            _addresses.Remove(address);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private void ClearOtherDefaults()
    {
        foreach (var address in _addresses.Where(a => a.IsDefault))
            address.UnsetAsDefault();
    }
}