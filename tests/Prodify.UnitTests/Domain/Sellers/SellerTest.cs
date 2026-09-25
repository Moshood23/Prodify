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


    [Fact]
    public void Reject_WithReason_StoresReasonAndDate()
    {
        var seller = Seller.Create("Test Store", "store@test.com");

        seller.Reject("Business details could not be verified.");

        Assert.Equal(SellerStatus.Rejected, seller.Status);
        Assert.Equal("Business details could not be verified.", seller.StatusReason);
        Assert.NotNull(seller.StatusChangedAt);
    }

    [Fact]
    public void Reinstate_ClearsSuspensionReason()
    {
        var seller = Seller.Create("Test Store", "store@test.com");
        seller.Approve();
        seller.Suspend("Too many late deliveries.");

        seller.Reinstate();

        Assert.Equal(SellerStatus.Approved, seller.Status);
        Assert.Null(seller.StatusReason);
    }

    [Fact]
    public void Reapply_AfterRejection_GoesBackToPendingWithNewDetails()
    {
        var seller = Seller.Create("Test Store", "store@test.com");
        seller.Reject("Please add a phone number.");

        seller.Reapply("Test Store Ltd", "08031234567", "Phones and accessories");

        Assert.Equal(SellerStatus.PendingVerification, seller.Status);
        Assert.Null(seller.StatusReason);
        Assert.Equal("Test Store Ltd", seller.BusinessName);
        Assert.Equal("08031234567", seller.PhoneNumber);
        Assert.Equal("Phones and accessories", seller.Description);
    }

    [Fact]
    public void Reapply_WhenNotRejected_ThrowsInvalidOperationException()
    {
        var seller = Seller.Create("Test Store", "store@test.com");

        Assert.Throws<InvalidOperationException>(() => seller.Reapply("Test Store", null, null));
    }
}