using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class SellerOrderItemConfiguration : IEntityTypeConfiguration<SellerOrderItem>
{
    public void Configure(EntityTypeBuilder<SellerOrderItem> builder)
    {
        builder.ToTable("SellerOrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SellerOrderId)
            .IsRequired();

        builder.Property(i => i.ProductVariantId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.OwnsOne(i => i.UnitPrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("UnitPriceAmount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            price.Property(p => p.Currency)
                .HasColumnName("UnitPriceCurrency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Ignore(i => i.Subtotal);

        builder.HasIndex(i => i.SellerOrderId);
    }
}