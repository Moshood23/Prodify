using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.ToTable("FlashSales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.SalePrice).HasColumnType("decimal(18,2)");
        builder.Property(s => s.RegularPrice).HasColumnType("decimal(18,2)");
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ReviewNote).HasMaxLength(FlashSale.MaxNoteLength);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(256);
        builder.Property(s => s.ModifiedBy).HasMaxLength(256);

        builder.HasOne<ProductVariant>().WithMany().HasForeignKey(s => s.ProductVariantId).OnDelete(DeleteBehavior.Restrict);

        // Finding the sales running now, and a seller's own sales.
        builder.HasIndex(s => new { s.Status, s.EndsAt });
        builder.HasIndex(s => s.ProductVariantId);
        builder.HasIndex(s => s.SellerId);

        builder.Ignore(s => s.UnitsLeft);
        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.DomainEvents);
    }
}

public class FlashSaleClaimConfiguration : IEntityTypeConfiguration<FlashSaleClaim>
{
    public void Configure(EntityTypeBuilder<FlashSaleClaim> builder)
    {
        builder.ToTable("FlashSaleClaims");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(256);
        builder.Property(c => c.ModifiedBy).HasMaxLength(256);

        builder.HasOne<FlashSale>().WithMany().HasForeignKey(c => c.FlashSaleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Order>().WithMany().HasForeignKey(c => c.OrderId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.FlashSaleId, c.CustomerId });
        builder.HasIndex(c => c.OrderId);

        builder.Ignore(c => c.DomainEvents);
    }
}
