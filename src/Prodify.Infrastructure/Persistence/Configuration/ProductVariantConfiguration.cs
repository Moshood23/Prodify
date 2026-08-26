using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Catalog.ValueObjects;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ProductId)
            .IsRequired();

        builder.Property(v => v.SKU)
            .HasConversion(
                sku => sku.Value,
                value => SKU.Create(value))
            .HasColumnName("SKU")
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(v => v.SKU)
            .IsUnique();

        builder.Property(v => v.Name)
            .HasMaxLength(200);

        builder.Property(v => v.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.CompareAtPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.Weight)
            .HasColumnType("decimal(10,3)");

        builder.Property(v => v.IsActive)
            .IsRequired();

        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.CreatedBy).HasMaxLength(256);
        builder.Property(v => v.ModifiedAt);
        builder.Property(v => v.ModifiedBy).HasMaxLength(256);

        builder.Ignore(v => v.DomainEvents);

        builder.HasIndex(v => v.ProductId);
    }
}