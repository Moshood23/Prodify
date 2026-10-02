using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.CreatedBy).HasMaxLength(256);
        builder.Property(w => w.ModifiedAt);
        builder.Property(w => w.ModifiedBy).HasMaxLength(256);

        builder.Ignore(w => w.DomainEvents);

        // A product is saved at most once per customer.
        builder.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
