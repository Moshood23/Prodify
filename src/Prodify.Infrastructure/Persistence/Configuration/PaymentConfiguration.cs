using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(a => a.Amount)
                .HasColumnName("Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            amount.Property(a => a.Currency)
                .HasColumnName("Currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(256);
        builder.Property(p => p.ModifiedAt);
        builder.Property(p => p.ModifiedBy).HasMaxLength(256);

        builder.Ignore(p => p.DomainEvents);

        builder.HasMany(p => p.Attempts)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Attempts)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_attempts");

        builder.HasIndex(p => p.OrderId);
    }
}