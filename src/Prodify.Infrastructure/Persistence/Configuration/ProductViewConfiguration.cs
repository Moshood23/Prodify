using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ProductViewConfiguration : IEntityTypeConfiguration<ProductView>
{
    public void Configure(EntityTypeBuilder<ProductView> builder)
    {
        builder.ToTable("ProductViews");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.CreatedBy).HasMaxLength(256);
        builder.Property(v => v.ModifiedAt);
        builder.Property(v => v.ModifiedBy).HasMaxLength(256);

        builder.Ignore(v => v.DomainEvents);

        // One row per customer and product; the newest first for "Recently viewed".
        builder.HasIndex(v => new { v.CustomerId, v.ProductId }).IsUnique();
        builder.HasIndex(v => new { v.CustomerId, v.ViewedAt });

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
