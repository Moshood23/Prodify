using Prodify.Domain.Payouts.Entities;

namespace Prodify.UnitTests.Payouts;

public class PayoutTests
{
    [Fact]
    public void Earning_TakesCommission_AndIsHeldForSevenDays()
    {
        var delivered = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

        var earning = SellerEarning.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 30_005m, 10m, delivered);

        Assert.Equal(3_000.50m, earning.Commission);
        Assert.Equal(27_004.50m, earning.NetAmount);
        Assert.Equal(delivered.AddDays(7), earning.AvailableAt);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901")]
    [InlineData("01234abcde")]
    public void Account_NeedsATenDigitNumber(string number)
    {
        Assert.Throws<ArgumentException>(() => PayoutAccount.Create(Guid.NewGuid(), "GTBank", number, "Ade Stores"));
    }

    [Fact]
    public void Payout_BelowMinimum_IsRefused()
    {
        var sellerId = Guid.NewGuid();
        var account = PayoutAccount.Create(sellerId, "GTBank", "0123456789", "Ade Stores");

        Assert.Throws<ArgumentOutOfRangeException>(() => SellerPayout.Request(sellerId, 4_999m, account));
    }

    [Fact]
    public void Payout_IsPaidOrRejectedOnce_AndKeepsTheBankDetails()
    {
        var sellerId = Guid.NewGuid();
        var account = PayoutAccount.Create(sellerId, "GTBank", "0123456789", "Ade Stores");
        var payout = SellerPayout.Request(sellerId, 20_000m, account);

        account.Update("Access Bank", "9876543210", "Ade Stores Ltd");
        payout.MarkPaid("TRF-001");

        Assert.Equal(PayoutStatus.Paid, payout.Status);
        Assert.Equal("0123456789", payout.AccountNumber);
        Assert.Throws<InvalidOperationException>(() => payout.Reject("Too late"));
    }

    [Fact]
    public void Commission_StaysWithinLimits()
    {
        var settings = PlatformSettings.CreateDefault();

        Assert.Equal(10m, settings.CommissionRate);
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.SetCommissionRate(60m));

        settings.SetCommissionRate(12.5m);
        Assert.Equal(12.5m, settings.CommissionRate);
    }
}
