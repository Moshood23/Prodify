using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Infrastructure.Email;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.ToTable("EmailMessages");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.ToAddress).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ToName).HasMaxLength(200);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.HtmlBody).IsRequired();
        builder.Property(e => e.TextBody).IsRequired();
        builder.Property(e => e.LastError).HasMaxLength(1000);

        // The sender looks for unsent emails that are due.
        builder.HasIndex(e => new { e.SentAt, e.NextAttemptAt });
    }
}
