using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Notifications.Entities;

namespace Prodify.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientId)
            .IsRequired();

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.ReadAt);

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.CreatedBy).HasMaxLength(256);
        builder.Property(n => n.ModifiedAt);
        builder.Property(n => n.ModifiedBy).HasMaxLength(256);

        builder.Ignore(n => n.DomainEvents);

        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.IsRead);
    }
}