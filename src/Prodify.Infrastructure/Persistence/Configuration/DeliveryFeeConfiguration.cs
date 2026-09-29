using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class DeliveryFeeConfiguration : IEntityTypeConfiguration<DeliveryFee>
{
    public void Configure(EntityTypeBuilder<DeliveryFee> builder)
    {
        builder.ToTable("DeliveryFees");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(f => f.State).IsUnique();

        builder.Property(f => f.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.CreatedBy).HasMaxLength(256);
        builder.Property(f => f.ModifiedAt);
        builder.Property(f => f.ModifiedBy).HasMaxLength(256);

        builder.Ignore(f => f.DomainEvents);
    }
}
