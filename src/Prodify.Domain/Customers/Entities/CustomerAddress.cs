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

    internal void SetAsDefault() => IsDefault = true;

    internal void UnsetAsDefault() => IsDefault = false;
}