using Prodify.Application.Ordering.Common;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.UnitTests.Promotions;

public class VoucherTests
{
    private static readonly OrderAddress Address =
        OrderAddress.Create("Chi Oke", "5 Allen Ave", "Ikeja", "Lagos", "Nigeria", "08031234567");

    // Two sellers: a 10,000 kettle and a 5,000 fan, delivered for 2,500.
    private static Order PlaceTwoSellerOrder() =>
        Order.Place(
            Guid.NewGuid(),
            Address,
            new[]
            {
                (Guid.NewGuid(), Guid.NewGuid(), "Kettle", 1, Money.Create(10_000m)),
                (Guid.NewGuid(), Guid.NewGuid(), "Fan", 1, Money.Create(5_000m)),
            },
            PaymentMethod.Card,
            deliveryFee: 2_500m);

    [Fact]
    public void Create_TidiesTheCode()
    {
        var voucher = Voucher.Create(" welcome10 ", "10% off", VoucherDiscountType.Percent, 10, 0);

        Assert.Equal("WELCOME10", voucher.Code);
        Assert.True(voucher.IsActive);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("HELLO WORLD")]
    [InlineData("SAVE-10")]
    public void Create_WithABadCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Voucher.Create(code, "Test", VoucherDiscountType.Fixed, 500, 0));
    }

    [Fact]
    public void Create_WithMoreThanNinetyPercent_Throws()
    {
        Assert.Throws<ArgumentException>(() => Voucher.Create("HALF", "Too much", VoucherDiscountType.Percent, 95, 0));
    }

    [Fact]
    public void Percent_IsWorkedOutOnTheItems()
    {
        var voucher = Voucher.Create("TEN", "10% off", VoucherDiscountType.Percent, 10, 0);

        Assert.Equal(1_550.50m, voucher.DiscountFor(15_505m));
    }

    [Fact]
    public void Fixed_IsNeverMoreThanTheItems()
    {
        var voucher = Voucher.Create("BIG", "5,000 off", VoucherDiscountType.Fixed, 5_000, 0);

        Assert.Equal(3_000m, voucher.DiscountFor(3_000m));
    }

    [Fact]
    public void Order_Total_TakesOffTheDiscount()
    {
        var order = PlaceTwoSellerOrder();
        order.ApplyVoucher("TEN", 1_500m);

        Assert.Equal(16_000m, order.Total.Amount);
        Assert.Equal("TEN", order.VoucherCode);
    }

    [Fact]
    public void Order_CannotTakeTwoVouchers()
    {
        var order = PlaceTwoSellerOrder();
        order.ApplyVoucher("TEN", 1_500m);

        Assert.Throws<InvalidOperationException>(() => order.ApplyVoucher("MORE", 500m));
    }

    [Fact]
    public void Refund_ForOnePart_GivesBackOnlyItsShareOfTheDiscount()
    {
        var order = PlaceTwoSellerOrder();
        order.ApplyVoucher("TEN", 1_500m);
        var kettle = order.SellerOrders.First();

        // The kettle is two thirds of the items, so it carries 1,000 of the 1,500 discount.
        Assert.Equal(9_000m, OrderRefunds.RefundFor(order, new[] { kettle }));
    }

    [Fact]
    public void Refund_ForEverything_IsWhatTheCustomerPaid()
    {
        var order = PlaceTwoSellerOrder();
        order.ApplyVoucher("TEN", 1_500m);

        Assert.Equal(order.Total.Amount, OrderRefunds.RefundFor(order, order.SellerOrders.ToList()));
    }
}
