using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts");

        builder.HasKey(a => a.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

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

        // Paystack payments are looked up by their reference when the customer comes back and on webhooks.
        builder.HasIndex(a => a.GatewayReference);
    }
}