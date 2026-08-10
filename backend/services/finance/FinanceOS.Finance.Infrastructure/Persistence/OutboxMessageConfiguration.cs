using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Finance.Infrastructure.Persistence;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).HasColumnName("id");
        builder.Property(message => message.Type).HasMaxLength(500).HasColumnName("type").IsRequired();
        builder.Property(message => message.Content).HasColumnName("content").IsRequired();
        builder.Property(message => message.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(message => message.ProcessedAt).HasColumnName("processed_at");
        builder.Property(message => message.Error).HasMaxLength(1000).HasColumnName("error");
        builder.HasIndex(message => message.ProcessedAt);
    }
}
