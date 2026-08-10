namespace FinanceOS.Identity.Application.Common;

public abstract class IdentityApplicationException(string message) : Exception(message);

public sealed class IdentityConflictException(string message) : IdentityApplicationException(message);

public sealed class IdentityNotFoundException(string message) : IdentityApplicationException(message);

public sealed class IdentityValidationException(string message) : IdentityApplicationException(message);
