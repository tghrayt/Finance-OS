using System.Text.Json;
using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Infrastructure.Persistence;

namespace FinanceOS.Finance.Infrastructure.Messaging;

internal sealed class OutboxWriter(FinanceDbContext dbContext) : IOutboxWriter
{
    public void Add<TMessage>(TMessage message)
        where TMessage : class
    {
        dbContext.OutboxMessages.Add(new OutboxMessage(
            Guid.NewGuid(),
            typeof(TMessage).AssemblyQualifiedName ?? typeof(TMessage).FullName ?? typeof(TMessage).Name,
            JsonSerializer.Serialize(message),
            DateTimeOffset.UtcNow));
    }
}
