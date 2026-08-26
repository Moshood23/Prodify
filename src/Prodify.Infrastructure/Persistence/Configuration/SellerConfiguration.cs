using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.ToTable("Sellers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(s => s.Email)
            .IsUnique();

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(256);
        builder.Property(s => s.ModifiedAt);
        builder.Property(s => s.ModifiedBy).HasMaxLength(256);

        builder.Ignore(s => s.DomainEvents);

        builder.HasMany(s => s.Addresses)
            .WithOne()
            .HasForeignKey(a => a.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_addresses");
    }
}