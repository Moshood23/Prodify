using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(256);
        builder.Property(p => p.ModifiedAt);
        builder.Property(p => p.ModifiedBy).HasMaxLength(256);

        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_images");

        builder.HasMany(p => p.Attributes)
            .WithOne()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Attributes)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_attributes");

        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BrandId);
    }
}