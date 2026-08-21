using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.Entities;

public sealed class OrderAddress : ValueObject
{
    public string RecipientName { get; }
    public string AddressLine1 { get; }
    public string? AddressLine2 { get; }
    public string City { get; }
    public string State { get; }
    public string? PostalCode { get; }
    public string Country { get; }
    public string PhoneNumber { get; }

    private OrderAddress(
        string recipientName,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string? postalCode,
        string country,
        string phoneNumber)
    {
        RecipientName = recipientName;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        PhoneNumber = phoneNumber;
    }

    public static OrderAddress Create(
        string recipientName,
        string addressLine1,
        string city,
        string state,
        string country,
        string phoneNumber,
        string? addressLine2 = null,
        string? postalCode = null)
    {
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

        return new OrderAddress(
            recipientName.Trim(),
            addressLine1.Trim(),
            addressLine2?.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode?.Trim(),
            country.Trim(),
            phoneNumber.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RecipientName;
        yield return AddressLine1;
        yield return AddressLine2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
        yield return PhoneNumber;
    }
}