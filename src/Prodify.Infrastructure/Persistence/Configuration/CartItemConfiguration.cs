using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Cart.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.CartId)
            .IsRequired();

        builder.Property(i => i.ProductVariantId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Ignore(i => i.Subtotal);

        builder.HasIndex(i => i.CartId);
    }
}