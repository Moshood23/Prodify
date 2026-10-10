using Prodify.Application.Ordering.Common;
using Prodify.Domain.Customers.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.UnitTests.Customers;

public class StoreCreditTests
{
    private static readonly OrderAddress Address =
        OrderAddress.Create("Chi Oke", "5 Allen Ave", "Ikeja", "Lagos", "Nigeria", "08031234567");

    // Two sellers: a 10,000 kettle and a 5,000 fan, delivered for 2,500.
    private static Order PlaceTwoSellerOrder(PaymentMethod paymentMethod = PaymentMethod.Card) =>
        Order.Place(
            Guid.NewGuid(),
            Address,
            new[]
            {
                (Guid.NewGuid(), Guid.NewGuid(), "Kettle", 1, Money.Create(10_000m)),
                (Guid.NewGuid(), Guid.NewGuid(), "Fan", 1, Money.Create(5_000m)),
            },
            paymentMethod,
            deliveryFee: 2_500m);

    [Fact]
    public void UseStoreCredit_TakesItOffWhatIsLeftToPay()
    {
        var order = PlaceTwoSellerOrder();

        order.UseStoreCredit(4_000m);

        Assert.Equal(17_500m, order.Value);
        Assert.Equal(13_500m, order.Total.Amount);
        Assert.Equal(4_000m, order.CreditLeft);
    }

    [Fact]
    public void UseStoreCredit_CanPayForTheWholeOrder()
    {
        var order = PlaceTwoSellerOrder();

        order.UseStoreCredit(17_500m);

        Assert.Equal(0m, order.Total.Amount);
    }

    [Fact]
    public void UseStoreCredit_MoreThanTheOrder_Throws()
    {
        var order = PlaceTwoSellerOrder();

        Assert.Throws<ArgumentOutOfRangeException>(() => order.UseStoreCredit(17_500.01m));
    }

    [Fact]
    public void UseStoreCredit_Twice_Throws()
    {
        var order = PlaceTwoSellerOrder();
        order.UseStoreCredit(1_000m);

        Assert.Throws<InvalidOperationException>(() => order.UseStoreCredit(1_000m));
    }

    [Fact]
    public void ReturnCredit_CannotGiveBackMoreThanWasUsed()
    {
        var order = PlaceTwoSellerOrder();
        order.UseStoreCredit(3_000m);

        order.ReturnCredit(2_000m);

        Assert.Equal(1_000m, order.CreditLeft);
        Assert.Throws<ArgumentOutOfRangeException>(() => order.ReturnCredit(1_000.01m));
    }

    [Fact]
    public void ValueLeftAfter_IsWhatTheOtherPartsStillCost()
    {
        var order = PlaceTwoSellerOrder();
        var kettle = order.SellerOrders.First(so => so.Items.Any(i => i.ProductName == "Kettle"));

        Assert.Equal(7_500m, OrderRefunds.ValueLeftAfter(order, new[] { kettle }));
        Assert.Equal(0m, OrderRefunds.ValueLeftAfter(order, order.SellerOrders.ToList()));
    }

    [Fact]
    public void Entries_AddAndSpend_HaveTheRightSign()
    {
        var customerId = Guid.NewGuid();

        var added = StoreCreditEntry.Add(customerId, 2_500m, "Refund", null);
        var spent = StoreCreditEntry.Spend(customerId, 1_000m, "Used on order", Guid.NewGuid());

        Assert.Equal(2_500m, added.Amount);
        Assert.Equal(StoreCreditKind.Refund, added.Kind);
        Assert.Equal(-1_000m, spent.Amount);
        Assert.Equal(StoreCreditKind.Spent, spent.Kind);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.005)]
    public void Entries_WithABadAmount_Throw(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StoreCreditEntry.Add(Guid.NewGuid(), amount, "Refund", null));
    }
}
