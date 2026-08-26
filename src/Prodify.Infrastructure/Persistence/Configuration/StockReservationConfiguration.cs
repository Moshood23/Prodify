using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.InventoryItemId)
            .IsRequired();

        builder.Property(r => r.Quantity)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.ReservedAt)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.HasIndex(r => r.InventoryItemId);
        builder.HasIndex(r => r.ExpiresAt);
    }
}