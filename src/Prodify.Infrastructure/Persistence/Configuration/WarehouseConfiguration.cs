using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.Property(w => w.Address)
            .HasMaxLength(500);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.CreatedBy).HasMaxLength(256);
        builder.Property(w => w.ModifiedAt);
        builder.Property(w => w.ModifiedBy).HasMaxLength(256);

        builder.Ignore(w => w.DomainEvents);
    }
}