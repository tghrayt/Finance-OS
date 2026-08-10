using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Features.Accounts.CreateAccount;
using FinanceOS.Finance.Application.Features.Categories.CreateCategory;
using FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;
using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;
using FinanceOS.Finance.Domain.Transactions;

namespace FinanceOS.Foundation.Tests;

public sealed class FinanceApplicationTests
{
    [Fact]
    public async Task CreateAccountPersistsAccount()
    {
        var accounts = new InMemoryAccountRepository();
        var handler = new CreateAccountHandler(accounts, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), "Checking", "Checking", 100, "EUR", null), CancellationToken.None);

        Assert.Equal("Checking", result.Name);
        Assert.Equal(100, result.CurrentBalance);
    }

    [Fact]
    public async Task CreateCategoryPersistsCategory()
    {
        var categories = new InMemoryCategoryRepository();
        var handler = new CreateCategoryHandler(categories, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(new CreateCategoryCommand(Guid.NewGuid(), "Groceries", null, "shopping-cart"), CancellationToken.None);

        Assert.Equal("Groceries", result.Name);
    }

    [Fact]
    public async Task CreateExpenseDebitsAccountAndWritesOutbox()
    {
        var householdId = Guid.NewGuid();
        var accounts = new InMemoryAccountRepository();
        var transactions = new InMemoryTransactionRepository();
        var outbox = new InMemoryOutboxWriter();
        var account = Account.Create(new HouseholdId(householdId), "Checking", AccountType.Checking, 100, "EUR");
        await accounts.AddAsync(account, CancellationToken.None);

        var handler = new CreateTransactionHandler(accounts, transactions, outbox, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new CreateTransactionCommand(householdId, account.Id.Value, null, "Expense", 25, "EUR", null, null, null, new DateOnly(2026, 8, 10), Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal("Expense", result.Type);
        Assert.Equal(75, account.CurrentBalance);
        Assert.Single(outbox.Messages);
    }

    [Fact]
    public async Task CreateTransferMovesBalanceBetweenAccounts()
    {
        var householdId = Guid.NewGuid();
        var accounts = new InMemoryAccountRepository();
        var source = Account.Create(new HouseholdId(householdId), "Checking", AccountType.Checking, 100, "EUR");
        var destination = Account.Create(new HouseholdId(householdId), "Savings", AccountType.Savings, 10, "EUR");
        await accounts.AddAsync(source, CancellationToken.None);
        await accounts.AddAsync(destination, CancellationToken.None);

        var handler = new CreateTransactionHandler(accounts, new InMemoryTransactionRepository(), new InMemoryOutboxWriter(), new InMemoryUnitOfWork());

        await handler.HandleAsync(
            new CreateTransactionCommand(householdId, source.Id.Value, destination.Id.Value, "Transfer", 40, "EUR", null, null, null, new DateOnly(2026, 8, 10), Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(60, source.CurrentBalance);
        Assert.Equal(50, destination.CurrentBalance);
    }

    private sealed class InMemoryAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts = [];
        public Task AddAsync(Account account, CancellationToken cancellationToken) { _accounts.Add(account); return Task.CompletedTask; }
        public Task<Account?> GetByIdAsync(AccountId id, CancellationToken cancellationToken) => Task.FromResult(_accounts.FirstOrDefault(account => account.Id == id));
        public Task<IReadOnlyCollection<Account>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Account>>(_accounts.Where(account => account.HouseholdId == householdId).ToArray());
    }

    private sealed class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories = [];
        public Task AddAsync(Category category, CancellationToken cancellationToken) { _categories.Add(category); return Task.CompletedTask; }
        public Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken) => Task.FromResult(_categories.FirstOrDefault(category => category.Id == id));
        public Task<IReadOnlyCollection<Category>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<Category>>(_categories.Where(category => category.HouseholdId == householdId).ToArray());
    }

    private sealed class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly List<FinancialTransaction> _transactions = [];
        public Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken) { _transactions.Add(transaction); return Task.CompletedTask; }
        public Task<IReadOnlyCollection<FinancialTransaction>> ListByHouseholdAsync(HouseholdId householdId, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<FinancialTransaction>>(_transactions.Where(transaction => transaction.HouseholdId == householdId).ToArray());
    }

    private sealed class InMemoryOutboxWriter : IOutboxWriter
    {
        public List<object> Messages { get; } = [];
        public void Add<TMessage>(TMessage message) where TMessage : class => Messages.Add(message);
    }

    private sealed class InMemoryUnitOfWork : IFinanceUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
