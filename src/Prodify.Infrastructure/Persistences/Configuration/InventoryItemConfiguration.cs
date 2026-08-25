using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductVariantId)
            .IsRequired();

        builder.Property(i => i.WarehouseId)
            .IsRequired();

        builder.Property(i => i.QuantityOnHand)
            .IsRequired();

        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(256);
        builder.Property(i => i.ModifiedAt);
        builder.Property(i => i.ModifiedBy).HasMaxLength(256);

        builder.Ignore(i => i.DomainEvents);
        builder.Ignore(i => i.QuantityReserved);
        builder.Ignore(i => i.AvailableQuantity);

        builder.HasMany(i => i.Reservations)
            .WithOne()
            .HasForeignKey(r => r.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Reservations)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_reservations");

        builder.HasMany(i => i.Movements)
            .WithOne()
            .HasForeignKey(m => m.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_movements");

        builder.HasIndex(i => new { i.ProductVariantId, i.WarehouseId })
            .IsUnique();
    }
}