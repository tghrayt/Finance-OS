using System.Text.Json;
using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Infrastructure.Persistence;

namespace FinanceOS.Budget.Infrastructure.Messaging;

internal sealed class OutboxWriter(BudgetDbContext dbContext) : IOutboxWriter
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
