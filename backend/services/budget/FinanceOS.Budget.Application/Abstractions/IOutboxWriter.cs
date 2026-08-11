namespace FinanceOS.Budget.Application.Abstractions;

public interface IOutboxWriter
{
    void Add<TMessage>(TMessage message)
        where TMessage : class;
}
