using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(i => i.Id);

        // The domain creates the Id. Without this, EF treats a new image added to an
        // existing product as an existing row and issues an UPDATE instead of an INSERT.
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(i => i.AltText)
            .HasMaxLength(250);

        builder.Property(i => i.DisplayOrder)
            .IsRequired();
    }
}