using Prodify.Domain.Common;

namespace Prodify.Domain.Sellers.Entities;

public class SellerAddress : Entity
{
    public Guid SellerId { get; private set; }
    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public string Country { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;

    private SellerAddress()
    {
    }

    internal SellerAddress(
        Guid id,
        Guid sellerId,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string? postalCode,
        string country,
        string phoneNumber) : base(id)
    {
        SellerId = sellerId;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        PhoneNumber = phoneNumber;
    }

    internal static SellerAddress Create(
        Guid sellerId,
        string addressLine1,
        string city,
        string state,
        string country,
        string phoneNumber,
        string? addressLine2 = null,
        string? postalCode = null)
    {
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

        return new SellerAddress(
            Guid.NewGuid(),
            sellerId,
            addressLine1.Trim(),
            addressLine2?.Trim(),
            city.Trim(),
            state.Trim(),
            postalCode?.Trim(),
            country.Trim(),
            phoneNumber.Trim());
    }
}