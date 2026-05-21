using BirthdayReminder.Domain.Entities;

namespace BirthdayReminder.Application.Interfaces;

public interface IUserRepository
{
    Task CreateUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteUserAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<List<User>> GetDueUsersAsync(DateTime utcNow, int batchSize, CancellationToken cancellationToken);
}
