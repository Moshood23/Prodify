using Prodify.Domain.Customers.Entities;

namespace Prodify.UnitTests.Domain.Customers;

public class ProductViewTests
{
    private static readonly DateTime Noon = new(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_KeepsCustomerProductAndTime()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var view = ProductView.Create(customerId, productId, Noon);

        Assert.Equal(customerId, view.CustomerId);
        Assert.Equal(productId, view.ProductId);
        Assert.Equal(Noon, view.ViewedAt);
    }

    [Fact]
    public void Create_WithoutCustomerOrProduct_Throws()
    {
        Assert.Throws<ArgumentException>(() => ProductView.Create(Guid.Empty, Guid.NewGuid(), Noon));
        Assert.Throws<ArgumentException>(() => ProductView.Create(Guid.NewGuid(), Guid.Empty, Noon));
    }

    [Fact]
    public void Seen_Later_MovesItToTheFront()
    {
        var view = ProductView.Create(Guid.NewGuid(), Guid.NewGuid(), Noon);

        view.Seen(Noon.AddHours(1));

        Assert.Equal(Noon.AddHours(1), view.ViewedAt);
    }

    [Fact]
    public void Seen_Earlier_KeepsTheLatestTime()
    {
        var view = ProductView.Create(Guid.NewGuid(), Guid.NewGuid(), Noon);

        view.Seen(Noon.AddDays(-2));

        Assert.Equal(Noon, view.ViewedAt);
    }
}
