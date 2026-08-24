using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class SellerAddressConfiguration : IEntityTypeConfiguration<SellerAddress>
{
    public void Configure(EntityTypeBuilder<SellerAddress> builder)
    {
        builder.ToTable("SellerAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SellerId)
            .IsRequired();

        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(300);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.SellerId);
    }
}