using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Abstractions;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);

    Task<User?> GetByExternalSubjectAsync(string externalSubject, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken);
}
