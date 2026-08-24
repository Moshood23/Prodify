using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.InventoryItemId)
            .IsRequired();

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Quantity)
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasMaxLength(500);

        builder.Property(m => m.OccurredAt)
            .IsRequired();

        builder.HasIndex(m => m.InventoryItemId);
    }
}