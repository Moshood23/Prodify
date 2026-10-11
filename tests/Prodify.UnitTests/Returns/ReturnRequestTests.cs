using Prodify.Domain.Payouts.Entities;
using Prodify.Domain.Returns.Entities;

namespace Prodify.UnitTests.Returns;

public class ReturnRequestTests
{
    // Two kettles at 10,000 each; the customer gets 19,000 back (1,000 of a voucher was Prodify's).
    private static ReturnRequest NewReturn() =>
        ReturnRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ReturnReason.Damaged, "  Arrived cracked  ",
            new[] { (Guid.NewGuid(), "Kettle", 2, 10_000m) },
            refundAmount: 19_000m);

    [Fact]
    public void Create_StartsAsRequested()
    {
        var request = NewReturn();

        Assert.Equal(ReturnStatus.Requested, request.Status);
        Assert.Equal("Arrived cracked", request.Details);
        Assert.Equal(20_000m, request.ItemsValue);
        Assert.True(request.IsOpen);
    }

    [Fact]
    public void Create_WithoutItems_Throws()
    {
        Assert.Throws<ArgumentException>(() => ReturnRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ReturnReason.Other, "x", Array.Empty<(Guid, string, int, decimal)>(), 0));
    }

    [Fact]
    public void Create_RefundingMoreThanTheItems_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ReturnRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ReturnReason.Damaged, null, new[] { (Guid.NewGuid(), "Kettle", 1, 10_000m) }, 10_000.01m));
    }

    [Fact]
    public void Reject_ThenEscalate_ThenRefund()
    {
        var request = NewReturn();

        request.Reject("Used, not faulty");
        Assert.Equal(ReturnStatus.Rejected, request.Status);
        Assert.False(request.IsOpen);

        request.Escalate("It was cracked in the box");
        Assert.Equal(ReturnStatus.Escalated, request.Status);
        Assert.True(request.IsOpen);

        request.MarkRefunded(15_000m, 4_000m, "Photos show the crack");
        Assert.Equal(ReturnStatus.Refunded, request.Status);
        Assert.Equal(15_000m, request.RefundedToCard);
        Assert.Equal(4_000m, request.RefundedAsCredit);
        Assert.NotNull(request.ResolvedAt);
    }

    [Fact]
    public void Reject_WithoutAReason_Throws()
    {
        Assert.Throws<ArgumentException>(() => NewReturn().Reject("  "));
    }

    [Fact]
    public void Escalate_BeforeTheSellerAnswered_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => NewReturn().Escalate(null));
    }

    [Fact]
    public void Refund_MoreThanTheReturnIsWorth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NewReturn().MarkRefunded(19_000m, 0.01m));
    }

    [Fact]
    public void Decline_ClosesTheReturn()
    {
        var request = NewReturn();

        request.Decline("Outside what we cover");

        Assert.Equal(ReturnStatus.Declined, request.Status);
        Assert.Throws<InvalidOperationException>(() => request.MarkRefunded(1m, 0));
    }

    [Fact]
    public void SellerEarning_RecordReturn_TakesOffTheSalesAndTheirCommission()
    {
        var earning = SellerEarning.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 30_000m, 10m, DateTime.UtcNow);
        earning.HoldForReturn();

        earning.RecordReturn(20_000m);
        earning.ReleaseHold();

        Assert.Equal(10_000m, earning.Sales);
        Assert.Equal(1_000m, earning.Commission);
        Assert.Equal(9_000m, earning.NetAmount);
        Assert.False(earning.HeldForReturn);
    }

    [Fact]
    public void SellerEarning_RecordReturn_MoreThanTheSales_Throws()
    {
        var earning = SellerEarning.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5_000m, 10m, DateTime.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() => earning.RecordReturn(5_000.01m));
    }
}
