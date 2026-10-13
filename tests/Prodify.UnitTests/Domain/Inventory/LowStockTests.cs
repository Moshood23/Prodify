using Prodify.Application.Inventory.LowStock;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.UnitTests.Domain.Inventory;

public class LowStockTests
{
    private static ProductVariant NewVariant() => ProductVariant.Create(Guid.NewGuid(), "LOW-1", 1000m);

    [Fact]
    public void NewVariant_WarnsAtFiveByDefault()
    {
        var variant = NewVariant();

        Assert.Equal(ProductVariant.DefaultLowStockThreshold, variant.LowStockThreshold);
        Assert.True(variant.IsLowStock(5));
        Assert.True(variant.IsLowStock(0));
        Assert.False(variant.IsLowStock(6));
    }

    [Fact]
    public void ThresholdZero_TurnsTheWarningOff()
    {
        var variant = NewVariant();
        variant.SetLowStockThreshold(0);

        Assert.False(variant.IsLowStock(0));
    }

    [Fact]
    public void InactiveVariant_IsNeverLow()
    {
        var variant = NewVariant();
        variant.Deactivate();

        Assert.False(variant.IsLowStock(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ProductVariant.MaxLowStockThreshold + 1)]
    public void Threshold_OutOfRange_Throws(int threshold)
    {
        Assert.Throws<ArgumentException>(() => NewVariant().SetLowStockThreshold(threshold));
    }

    [Fact]
    public void AlertFlag_SetAndCleared()
    {
        var variant = NewVariant();

        variant.MarkLowStockAlertSent();
        Assert.True(variant.LowStockAlertSent);

        variant.ClearLowStockAlert();
        Assert.False(variant.LowStockAlertSent);
    }

    [Theory]
    [InlineData("2026-10-13T06:59:00", "2026-10-12T07:00:00")]
    [InlineData("2026-10-13T07:00:00", "2026-10-13T07:00:00")]
    [InlineData("2026-10-13T23:30:00", "2026-10-13T07:00:00")]
    public void DailyEmailCutoff_IsTheLatest7amUtc(string now, string expected)
    {
        Assert.Equal(DateTime.Parse(expected), LowStockRules.DailyEmailCutoff(DateTime.Parse(now)));
    }

    [Theory]
    [InlineData(0, "Out of stock")]
    [InlineData(3, "3 left")]
    public void Describe_ShowsUnitsLeft(int available, string expected)
    {
        Assert.Equal(expected, LowStockRules.Describe(available));
    }
}
