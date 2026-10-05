using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Payouts.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class PlatformSettingsConfiguration : IEntityTypeConfiguration<PlatformSettings>
{
    public void Configure(EntityTypeBuilder<PlatformSettings> builder)
    {
        builder.ToTable("PlatformSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.CommissionRate).HasColumnType("decimal(5,2)");
        builder.Property(s => s.CreatedBy).HasMaxLength(256);
        builder.Property(s => s.ModifiedBy).HasMaxLength(256);
        builder.Ignore(s => s.DomainEvents);
    }
}

public class SellerEarningConfiguration : IEntityTypeConfiguration<SellerEarning>
{
    public void Configure(EntityTypeBuilder<SellerEarning> builder)
    {
        builder.ToTable("SellerEarnings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Sales).HasColumnType("decimal(18,2)");
        builder.Property(e => e.CommissionRate).HasColumnType("decimal(5,2)");
        builder.Property(e => e.Commission).HasColumnType("decimal(18,2)");
        builder.Property(e => e.NetAmount).HasColumnType("decimal(18,2)");

        // One earning per delivered part.
        builder.HasIndex(e => e.SellerOrderId).IsUnique();
        builder.HasIndex(e => new { e.SellerId, e.AvailableAt });

        builder.HasOne<Seller>().WithMany().HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SellerOrder>().WithMany().HasForeignKey(e => e.SellerOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayoutAccountConfiguration : IEntityTypeConfiguration<PayoutAccount>
{
    public void Configure(EntityTypeBuilder<PayoutAccount> builder)
    {
        builder.ToTable("PayoutAccounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.BankName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(10);
        builder.Property(a => a.AccountName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.CreatedBy).HasMaxLength(256);
        builder.Property(a => a.ModifiedBy).HasMaxLength(256);
        builder.Ignore(a => a.DomainEvents);

        builder.HasIndex(a => a.SellerId).IsUnique();
        builder.HasOne<Seller>().WithMany().HasForeignKey(a => a.SellerId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SellerPayoutConfiguration : IEntityTypeConfiguration<SellerPayout>
{
    public void Configure(EntityTypeBuilder<SellerPayout> builder)
    {
        builder.ToTable("SellerPayouts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.BankName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.AccountNumber).IsRequired().HasMaxLength(10);
        builder.Property(p => p.AccountName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.RejectReason).HasMaxLength(500);
        builder.Property(p => p.CreatedBy).HasMaxLength(256);
        builder.Property(p => p.ModifiedBy).HasMaxLength(256);
        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => new { p.Status, p.CreatedAt });

        // At most one open request per seller, enforced by the database.
        builder.HasIndex(p => p.SellerId)
            .IsUnique()
            .HasFilter("[Status] = 'Requested'")
            .HasDatabaseName("IX_SellerPayouts_SellerId_OpenRequest");

        builder.HasOne<Seller>().WithMany().HasForeignKey(p => p.SellerId).OnDelete(DeleteBehavior.Restrict);
    }
}
