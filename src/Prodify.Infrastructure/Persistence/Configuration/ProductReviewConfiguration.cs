using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ReviewerName).IsRequired().HasMaxLength(120);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(ProductReview.MaxTitleLength);
        builder.Property(r => r.Comment).HasMaxLength(ProductReview.MaxCommentLength);
        builder.Property(r => r.HiddenReason).HasMaxLength(500);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(256);
        builder.Property(r => r.ModifiedAt);
        builder.Property(r => r.ModifiedBy).HasMaxLength(256);

        builder.Ignore(r => r.DomainEvents);

        // One review per customer per product.
        builder.HasIndex(r => new { r.ProductId, r.CustomerId }).IsUnique();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
