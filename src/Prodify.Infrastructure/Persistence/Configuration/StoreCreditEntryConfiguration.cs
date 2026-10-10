using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class StoreCreditEntryConfiguration : IEntityTypeConfiguration<StoreCreditEntry>
{
    public void Configure(EntityTypeBuilder<StoreCreditEntry> builder)
    {
        builder.ToTable("StoreCreditEntries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Kind).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(200);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.ModifiedBy).HasMaxLength(256);

        builder.HasIndex(e => e.CustomerId);

        builder.Ignore(e => e.DomainEvents);
    }
}
