namespace FinanceOS.Finance.Application.Common;

public abstract class FinanceApplicationException(string message) : Exception(message);

public sealed class FinanceNotFoundException(string message) : FinanceApplicationException(message);

public sealed class FinanceValidationException(string message) : FinanceApplicationException(message);
