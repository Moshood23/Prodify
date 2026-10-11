using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.ToTable("ReturnRequests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Reason).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Details).HasMaxLength(ReturnRequest.MaxDetailsLength);
        builder.Property(r => r.SellerNote).HasMaxLength(ReturnRequest.MaxNoteLength);
        builder.Property(r => r.CustomerNote).HasMaxLength(ReturnRequest.MaxNoteLength);
        builder.Property(r => r.AdminNote).HasMaxLength(ReturnRequest.MaxNoteLength);

        builder.Property(r => r.RefundAmount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.RefundedToCard).HasColumnType("decimal(18,2)");
        builder.Property(r => r.RefundedAsCredit).HasColumnType("decimal(18,2)");

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(256);
        builder.Property(r => r.ModifiedBy).HasMaxLength(256);

        // One return per delivered part of an order.
        builder.HasIndex(r => r.SellerOrderId).IsUnique();
        builder.HasIndex(r => new { r.SellerId, r.Status });
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.Status);

        builder.HasOne<SellerOrder>().WithMany().HasForeignKey(r => r.SellerOrderId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.ReturnRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(r => r.ItemsValue);
        builder.Ignore(r => r.IsOpen);
        builder.Ignore(r => r.DomainEvents);
    }
}

public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> builder)
    {
        builder.ToTable("ReturnItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
    }
}
