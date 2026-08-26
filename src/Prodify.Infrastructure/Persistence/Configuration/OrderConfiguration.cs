using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.OwnsOne(o => o.OrderNumber, num =>
        {
            num.Property(n => n.Value)
                .HasColumnName("OrderNumber")
                .IsRequired()
                .HasMaxLength(50);

            num.HasIndex(n => n.Value).IsUnique();
        });

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.OwnsOne(o => o.ShippingAddress, addr =>
        {
            addr.Property(a => a.RecipientName).HasColumnName("ShippingRecipientName").IsRequired().HasMaxLength(200);
            addr.Property(a => a.AddressLine1).HasColumnName("ShippingAddressLine1").IsRequired().HasMaxLength(300);
            addr.Property(a => a.AddressLine2).HasColumnName("ShippingAddressLine2").HasMaxLength(300);
            addr.Property(a => a.City).HasColumnName("ShippingCity").IsRequired().HasMaxLength(100);
            addr.Property(a => a.State).HasColumnName("ShippingState").IsRequired().HasMaxLength(100);
            addr.Property(a => a.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(20);
            addr.Property(a => a.Country).HasColumnName("ShippingCountry").IsRequired().HasMaxLength(100);
            addr.Property(a => a.PhoneNumber).HasColumnName("ShippingPhoneNumber").IsRequired().HasMaxLength(20);
        });

        builder.Property(o => o.IsPaid)
            .IsRequired();

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.CreatedBy).HasMaxLength(256);
        builder.Property(o => o.ModifiedAt);
        builder.Property(o => o.ModifiedBy).HasMaxLength(256);

        builder.Ignore(o => o.DomainEvents);
        builder.Ignore(o => o.Total);
        builder.Ignore(o => o.Status);

        builder.HasMany(o => o.SellerOrders)
            .WithOne()
            .HasForeignKey(so => so.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.SellerOrders)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_sellerOrders");

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");

        builder.HasIndex(o => o.CustomerId);
    }
}