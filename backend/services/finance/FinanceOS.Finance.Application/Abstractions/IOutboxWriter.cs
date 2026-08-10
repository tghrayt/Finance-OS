namespace FinanceOS.Finance.Application.Abstractions;

public interface IOutboxWriter
{
    void Add<TMessage>(TMessage message)
        where TMessage : class;
}
