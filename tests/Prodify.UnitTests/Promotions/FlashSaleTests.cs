using Prodify.Application.Promotions.FlashSales.Common;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.UnitTests.Promotions;

public class FlashSaleTests
{
    private static readonly DateTime Now = new(2026, 10, 11, 9, 0, 0, DateTimeKind.Utc);

    // 20 phones at 90,000 instead of 100,000, at most 2 each, for a day starting in an hour.
    private static FlashSale NewSale(decimal salePrice = 90_000m, int quantity = 20, int maxPerCustomer = 2,
        DateTime? startsAt = null, DateTime? endsAt = null)
    {
        var start = startsAt ?? Now.AddHours(1);
        return FlashSale.Create(Guid.NewGuid(), Guid.NewGuid(), 100_000m, salePrice, quantity, maxPerCustomer,
            start, endsAt ?? start.AddDays(1), Now, maxPerCustomerLimit: 10);
    }

    private static FlashSale LiveSale(int quantity = 20, int maxPerCustomer = 2)
    {
        var sale = NewSale(quantity: quantity, maxPerCustomer: maxPerCustomer, startsAt: Now);
        sale.Approve(Now);
        return sale;
    }

    [Fact]
    public void Create_StartsAsPending()
    {
        var sale = NewSale();

        Assert.Equal(FlashSaleStatus.Pending, sale.Status);
        Assert.Equal(20, sale.UnitsLeft);
        Assert.False(sale.IsRunningAt(Now.AddHours(2)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(95_001)]
    [InlineData(100_000)]
    public void Create_SalePriceNotAtLeastFivePercentOff_Throws(decimal salePrice)
    {
        Assert.Throws<ArgumentException>(() => NewSale(salePrice: salePrice));
    }

    [Fact]
    public void Create_FivePercentOff_IsAllowed()
    {
        Assert.Equal(95_000m, NewSale(salePrice: 95_000m).SalePrice);
    }

    [Fact]
    public void Create_LimitPerCustomerAboveUnits_Throws()
    {
        Assert.Throws<ArgumentException>(() => NewSale(quantity: 1, maxPerCustomer: 2));
    }

    [Fact]
    public void Create_InThePast_Throws()
    {
        Assert.Throws<ArgumentException>(() => NewSale(startsAt: Now.AddHours(-1)));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(24 * 7 + 1)]
    public void Create_TooShortOrTooLong_Throws(double hours)
    {
        Assert.Throws<ArgumentException>(() => NewSale(startsAt: Now, endsAt: Now.AddHours(hours)));
    }

    [Fact]
    public void Approve_RunsBetweenStartAndEnd()
    {
        var sale = NewSale();
        sale.Approve(Now);

        Assert.False(sale.IsRunningAt(Now));
        Assert.True(sale.IsRunningAt(sale.StartsAt));
        Assert.False(sale.IsRunningAt(sale.EndsAt));
    }

    [Fact]
    public void Approve_AfterTheEnd_Throws()
    {
        var sale = NewSale();

        Assert.Throws<InvalidOperationException>(() => sale.Approve(sale.EndsAt));
    }

    [Fact]
    public void Reject_NeedsAReason()
    {
        var sale = NewSale();

        Assert.Throws<ArgumentException>(() => sale.Reject("  ", Now));
        sale.Reject("  Not a real discount  ", Now);

        Assert.Equal(FlashSaleStatus.Rejected, sale.Status);
        Assert.Equal("Not a real discount", sale.ReviewNote);
        Assert.Throws<InvalidOperationException>(() => sale.Approve(Now));
    }

    [Fact]
    public void Stop_OnlyAnApprovedSale()
    {
        var sale = NewSale();
        Assert.Throws<InvalidOperationException>(() => sale.Stop("Complaint", Now));

        sale.Approve(Now);
        sale.Stop("Complaint", Now);

        Assert.Equal(FlashSaleStatus.Stopped, sale.Status);
        Assert.False(sale.IsRunningAt(sale.StartsAt));
    }

    [Fact]
    public void Cancel_BeforeTheEnd_Only()
    {
        var sale = LiveSale();
        Assert.Throws<InvalidOperationException>(() => sale.Cancel(sale.EndsAt));

        sale.Cancel(Now);
        Assert.Equal(FlashSaleStatus.Cancelled, sale.Status);
    }

    [Fact]
    public void Claim_TakesUnits_AndReleaseGivesThemBack()
    {
        var sale = LiveSale(quantity: 3);

        var claim = FlashSaleClaim.Create(sale, Guid.NewGuid(), Guid.NewGuid(), 2, Now);
        Assert.Equal(1, sale.UnitsLeft);

        Assert.Throws<InvalidOperationException>(() => FlashSaleClaim.Create(sale, Guid.NewGuid(), Guid.NewGuid(), 2, Now));

        claim.Release(sale);
        claim.Release(sale);
        Assert.True(claim.IsReleased);
        Assert.Equal(3, sale.UnitsLeft);
    }

    [Fact]
    public void Claim_WhenNotRunning_Throws()
    {
        var sale = NewSale();
        sale.Approve(Now);

        Assert.Throws<InvalidOperationException>(() => FlashSaleClaim.Create(sale, Guid.NewGuid(), Guid.NewGuid(), 1, Now));
    }

    [Theory]
    [InlineData(20, 0, 0, 2)]   // plenty left: the per-customer limit
    [InlineData(20, 19, 0, 1)]  // one unit left
    [InlineData(20, 0, 1, 1)]   // already bought one
    [InlineData(20, 0, 2, 0)]   // had their share
    [InlineData(20, 20, 0, 0)]  // sold out
    public void Offer_Allowance(int quantity, int sold, int bought, int allowance)
    {
        var offer = new FlashSaleOffer(Guid.NewGuid(), 90_000m, 100_000m, Now, quantity, sold, 2, bought);

        Assert.Equal(allowance, offer.Allowance);
        Assert.Equal(allowance >= 1, offer.Covers(1));
        Assert.False(offer.Covers(allowance + 1));
        Assert.False(offer.Covers(0));
    }
}
