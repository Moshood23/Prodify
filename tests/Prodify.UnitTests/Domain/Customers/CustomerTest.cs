using Prodify.Domain.Customers.Entities;

namespace Prodify.UnitTests.Domain.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidData_CreatesCustomer()
    {
        var customer = Customer.Create("John", "Doe", "john@example.com");

        Assert.Equal("John Doe", customer.FullName);
        Assert.True(customer.IsActive);
        Assert.Empty(customer.Addresses);
    }

    [Fact]
    public void Create_NormalizesEmailToLowercase()
    {
        var customer = Customer.Create("John", "Doe", "John@Example.COM");

        Assert.Equal("john@example.com", customer.Email);
    }

    [Fact]
    public void Create_WithEmptyEmail_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Customer.Create("John", "Doe", string.Empty));
    }

    [Fact]
    public void AddAddress_FirstAddress_IsAutomaticallyDefault()
    {
        var customer = Customer.Create("John", "Doe", "john@example.com");
        var address = customer.AddAddress("Home", "John Doe", "123 Main St", "Lagos", "Lagos", "Nigeria", "08012345678");

        Assert.True(address.IsDefault);
    }

    [Fact]
    public void AddAddress_SecondAddressAsDefault_ClearsFirstDefault()
    {
        var customer = Customer.Create("John", "Doe", "john@example.com");
        var first = customer.AddAddress("Home", "John Doe", "123 Main St", "Lagos", "Lagos", "Nigeria", "08012345678");
        var second = customer.AddAddress("Office", "John Doe", "456 Work Ave", "Lagos", "Lagos", "Nigeria", "08012345678", setAsDefault: true);

        Assert.False(first.IsDefault);
        Assert.True(second.IsDefault);
    }

    [Fact]
    public void SetDefaultAddress_ChangesDefaultCorrectly()
    {
        var customer = Customer.Create("John", "Doe", "john@example.com");
        var first = customer.AddAddress("Home", "John Doe", "123 Main St", "Lagos", "Lagos", "Nigeria", "08012345678");
        var second = customer.AddAddress("Office", "John Doe", "456 Work Ave", "Lagos", "Lagos", "Nigeria", "08012345678");

        customer.SetDefaultAddress(second.Id);

        Assert.False(first.IsDefault);
        Assert.True(second.IsDefault);
    }

    [Fact]
    public void RemoveAddress_RemovesFromCollection()
    {
        var customer = Customer.Create("John", "Doe", "john@example.com");
        var address = customer.AddAddress("Home", "John Doe", "123 Main St", "Lagos", "Lagos", "Nigeria", "08012345678");

        customer.RemoveAddress(address.Id);

        Assert.Empty(customer.Addresses);
    }

    [Fact]
    public void UpdateAddress_ChangesDetails()
    {
        var customer = Customer.Create("Ada", "Obi", "ada@test.com");
        var address = customer.AddAddress("Home", "Ada Obi", "1 Allen Avenue", "Ikeja", "Lagos", "Nigeria", "08030000000");

        customer.UpdateAddress(address.Id, "Office", "Ada Obi", "5 Marina", "", "Lagos Island", "Lagos", null, "Nigeria", "08031111111");

        Assert.Equal("Office", address.Label);
        Assert.Equal("5 Marina", address.AddressLine1);
        Assert.Null(address.AddressLine2);
        Assert.Equal("08031111111", address.PhoneNumber);
    }

    [Fact]
    public void RemoveAddress_WhenDefaultRemoved_NextAddressBecomesDefault()
    {
        var customer = Customer.Create("Ada", "Obi", "ada@test.com");
        var home = customer.AddAddress("Home", "Ada Obi", "1 Allen Avenue", "Ikeja", "Lagos", "Nigeria", "08030000000");
        var office = customer.AddAddress("Office", "Ada Obi", "5 Marina", "Lagos Island", "Lagos", "Nigeria", "08030000000");

        customer.RemoveAddress(home.Id);

        Assert.Single(customer.Addresses);
        Assert.True(office.IsDefault);
    }
}