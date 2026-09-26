using Prodify.Domain.Common;

namespace Prodify.Domain.Customers.Entities;

public class CustomerAddress : Entity
{
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = null!;
    public string RecipientName { get; private set; } = null!;
    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public string Country { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private CustomerAddress()
    {
    }

    internal CustomerAddress(
        Guid id,
        Guid customerId,
        string label,
        string recipientName,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string? postalCode,
        string country,
        string phoneNumber,
        bool isDefault) : base(id)
    {
        CustomerId = customerId;
        Label = label;
        RecipientName = recipientName;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        PhoneNumber = phoneNumber;
        IsDefault = isDefault;
    }

    internal static CustomerAddress Create(
        Guid customerId,
        string label,
        string recipientName,
        string addressLine1,
        string city,
        string state,
        string country,
        string phoneNumber,
        string? addressLine2 = null,
        string? postalCode = null,
        bool isDefault = false)
    {
        EnsureValid(label, recipientName, addressLine1, city, state, country, phoneNumber);

        return new CustomerAddress(
            Guid.NewGuid(),
            customerId,
            label.Trim(),
            recipientName.Trim(),
            addressLine1.Trim(),
            addressLine2?.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode?.Trim(),
            country.Trim(),
            phoneNumber.Trim(),
            isDefault);
    }

    internal void Update(
        string label,
        string recipientName,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string? postalCode,
        string country,
        string phoneNumber)
    {
        EnsureValid(label, recipientName, addressLine1, city, state, country, phoneNumber);

        Label = label.Trim();
        RecipientName = recipientName.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        Country = country.Trim();
        PhoneNumber = phoneNumber.Trim();
    }

    internal void SetAsDefault() => IsDefault = true;

    internal void UnsetAsDefault() => IsDefault = false;

    private static void EnsureValid(
        string label, string recipientName, string addressLine1, string city, string state, string country, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Address label cannot be empty.", nameof(label));

        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Recipient name cannot be empty.", nameof(recipientName));

        if (string.IsNullOrWhiteSpace(addressLine1))
            throw new ArgumentException("Address line 1 cannot be empty.", nameof(addressLine1));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.", nameof(state));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));
    }
}