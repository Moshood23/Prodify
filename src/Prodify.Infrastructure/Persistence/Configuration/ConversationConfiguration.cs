using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prodify.Domain.Messaging.Entities;

namespace Prodify.Infrastructure.Persistence.Configuration;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Subject).IsRequired().HasMaxLength(Conversation.MaxSubjectLength);
        builder.Property(c => c.LastMessagePreview).IsRequired().HasMaxLength(Conversation.PreviewLength + 3);
        builder.Property(c => c.LastSender).IsRequired().HasConversion<string>().HasMaxLength(10);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(256);
        builder.Property(c => c.ModifiedBy).HasMaxLength(256);

        // One conversation per customer, seller and topic; the inboxes list the latest first.
        // No filter: SQL Server treats the empty product or order as a value, so the index covers every conversation.
        builder.HasIndex(c => new { c.CustomerId, c.SellerId, c.ProductId, c.OrderId }).IsUnique().HasFilter(null);
        builder.HasIndex(c => new { c.CustomerId, c.LastMessageAt });
        builder.HasIndex(c => new { c.SellerId, c.LastMessageAt });

        builder.Ignore(c => c.DomainEvents);
    }
}

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("ConversationMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Body).IsRequired().HasMaxLength(ConversationMessage.MaxLength);
        builder.Property(m => m.Sender).IsRequired().HasConversion<string>().HasMaxLength(10);

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(256);
        builder.Property(m => m.ModifiedBy).HasMaxLength(256);

        builder.HasOne<Conversation>().WithMany().HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => new { m.ConversationId, m.SentAt });

        builder.Ignore(m => m.DomainEvents);
    }
}
