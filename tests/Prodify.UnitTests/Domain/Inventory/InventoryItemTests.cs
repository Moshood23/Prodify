using Prodify.Domain.Inventory.Entities;

namespace Prodify.UnitTests.Domain.Inventory;

public class InventoryItemTests
{
    [Fact]
    public void Create_WithInitialQuantity_SetsQuantityOnHand()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 100);

        Assert.Equal(100, item.QuantityOnHand);
        Assert.Equal(100, item.AvailableQuantity);
        Assert.Single(item.Movements);
    }

    [Fact]
    public void Reserve_WithinAvailableQuantity_CreatesReservation()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 100);
        var reservation = item.Reserve(30, TimeSpan.FromMinutes(15));

        Assert.Equal(30, reservation.Quantity);
        Assert.Equal(30, item.QuantityReserved);
        Assert.Equal(70, item.AvailableQuantity);
        Assert.Equal(100, item.QuantityOnHand);
    }

    [Fact]
    public void Reserve_ExceedingAvailableQuantity_ThrowsInvalidOperationException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 50);

        Assert.Throws<InvalidOperationException>(() =>
            item.Reserve(51, TimeSpan.FromMinutes(15)));
    }

    [Fact]
    public void ConfirmReservation_ReducesQuantityOnHand()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 100);
        var reservation = item.Reserve(30, TimeSpan.FromMinutes(15));

        item.ConfirmReservation(reservation.Id);

        Assert.Equal(70, item.QuantityOnHand);
        Assert.Equal(0, item.QuantityReserved);
        Assert.Equal(70, item.AvailableQuantity);
    }

    [Fact]
    public void ReleaseReservation_RestoresAvailableQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 100);
        var reservation = item.Reserve(30, TimeSpan.FromMinutes(15));

        item.ReleaseReservation(reservation.Id);

        Assert.Equal(100, item.QuantityOnHand);
        Assert.Equal(0, item.QuantityReserved);
        Assert.Equal(100, item.AvailableQuantity);
    }

    [Fact]
    public void AdjustStock_WithNegativeResultingQuantity_ThrowsInvalidOperationException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10);

        Assert.Throws<InvalidOperationException>(() =>
            item.AdjustStock(-20));
    }

    [Fact]
    public void MultipleReservations_TrackAvailableQuantityCorrectly()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 100);
        item.Reserve(30, TimeSpan.FromMinutes(15));
        item.Reserve(40, TimeSpan.FromMinutes(15));

        Assert.Equal(70, item.QuantityReserved);
        Assert.Equal(30, item.AvailableQuantity);
    }
}