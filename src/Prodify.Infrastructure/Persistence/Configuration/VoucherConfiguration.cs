using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Code).IsRequired().HasMaxLength(Voucher.MaxCodeLength);
        builder.HasIndex(v => v.Code).IsUnique();

        builder.Property(v => v.Description).IsRequired().HasMaxLength(200);
        builder.Property(v => v.DiscountType).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(v => v.Value).HasColumnType("decimal(18,2)");
        builder.Property(v => v.MinOrderAmount).HasColumnType("decimal(18,2)");

        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.CreatedBy).HasMaxLength(256);
        builder.Property(v => v.ModifiedBy).HasMaxLength(256);

        builder.Ignore(v => v.DomainEvents);
    }
}
