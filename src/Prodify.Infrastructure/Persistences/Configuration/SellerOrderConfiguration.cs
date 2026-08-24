using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class SellerOrderConfiguration : IEntityTypeConfiguration<SellerOrder>
{
    public void Configure(EntityTypeBuilder<SellerOrder> builder)
    {
        builder.ToTable("SellerOrders");

        builder.HasKey(so => so.Id);

        builder.Property(so => so.OrderId)
            .IsRequired();

        builder.Property(so => so.SellerId)
            .IsRequired();

        builder.Property(so => so.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(so => so.CreatedAt).IsRequired();
        builder.Property(so => so.CreatedBy).HasMaxLength(256);
        builder.Property(so => so.ModifiedAt);
        builder.Property(so => so.ModifiedBy).HasMaxLength(256);

        builder.Ignore(so => so.DomainEvents);
        builder.Ignore(so => so.Total);

        builder.HasMany(so => so.Items)
            .WithOne()
            .HasForeignKey(i => i.SellerOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(so => so.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_items");

        builder.HasMany(so => so.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.SellerOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(so => so.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_statusHistory");

        builder.HasIndex(so => so.OrderId);
        builder.HasIndex(so => so.SellerId);
    }
}