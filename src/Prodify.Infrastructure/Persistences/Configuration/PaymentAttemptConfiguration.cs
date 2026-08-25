using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PaymentId)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.FailureReason)
            .HasMaxLength(1000);

        builder.Property(a => a.GatewayReference)
            .HasMaxLength(256);

        builder.Property(a => a.AttemptedAt)
            .IsRequired();

        builder.HasIndex(a => a.PaymentId);
    }
}