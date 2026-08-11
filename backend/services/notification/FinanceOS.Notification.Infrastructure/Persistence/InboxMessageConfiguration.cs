using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Notification.Infrastructure.Persistence;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(message => new { message.MessageId, message.ConsumerName });
        builder.Property(message => message.MessageId).HasColumnName("message_id");
        builder.Property(message => message.ConsumerName).HasMaxLength(160).HasColumnName("consumer_name");
        builder.Property(message => message.ProcessedAt).HasColumnName("processed_at").IsRequired();
    }
}
