using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Cart.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Domain.Cart.Entities.Cart>
{
    public void Configure(EntityTypeBuilder<Domain.Cart.Entities.Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerId);

        builder.Property(c => c.SessionId)
            .HasMaxLength(256);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(256);
        builder.Property(c => c.ModifiedAt);
        builder.Property(c => c.ModifiedBy).HasMaxLength(256);

        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.Total);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");

        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.SessionId);
    }
}