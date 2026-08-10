namespace FinanceOS.Budget.Application.Common;

public abstract class BudgetApplicationException(string message) : Exception(message);

public sealed class BudgetValidationException(string message) : BudgetApplicationException(message);

public sealed class BudgetNotFoundException(string message) : BudgetApplicationException(message);

public sealed class BudgetConflictException(string message) : BudgetApplicationException(message);
