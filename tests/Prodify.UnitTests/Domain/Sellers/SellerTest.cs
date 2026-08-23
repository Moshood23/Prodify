using Prodify.Domain.Sellers.Entities;

namespace Prodify.UnitTests.Domain.Sellers;

public class SellerTests
{
    [Fact]
    public void Create_SetsInitialStatusPendingVerification()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");

        Assert.Equal(SellerStatus.PendingVerification, seller.Status);
    }

    [Fact]
    public void Approve_FromPendingVerification_SetsStatusApproved()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");
        seller.Approve();

        Assert.Equal(SellerStatus.Approved, seller.Status);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ThrowsInvalidOperationException()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");
        seller.Approve();

        Assert.Throws<InvalidOperationException>(() => seller.Approve());
    }

    [Fact]
    public void Suspend_FromApproved_SetsStatusSuspended()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");
        seller.Approve();
        seller.Suspend();

        Assert.Equal(SellerStatus.Suspended, seller.Status);
    }

    [Fact]
    public void Suspend_FromPendingVerification_ThrowsInvalidOperationException()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");

        Assert.Throws<InvalidOperationException>(() => seller.Suspend());
    }

    [Fact]
    public void Reinstate_FromSuspended_SetsStatusApproved()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");
        seller.Approve();
        seller.Suspend();
        seller.Reinstate();

        Assert.Equal(SellerStatus.Approved, seller.Status);
    }

    [Fact]
    public void AddAddress_AddsToCollection()
    {
        var seller = Seller.Create("Acme Store", "seller@acme.com");
        seller.AddAddress("123 Warehouse Rd", "Lagos", "Lagos", "Nigeria", "08012345678");

        Assert.Single(seller.Addresses);
    }
}