using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CustomerId)
            .IsRequired();

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.RecipientName)
            .IsRequired()
            .HasMaxLength(200);

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

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.HasIndex(a => a.CustomerId);
    }
}