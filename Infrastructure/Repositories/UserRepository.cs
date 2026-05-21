using BirthdayReminder.Application.DTOs;
using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Infrastructure.Persistences;
using Microsoft.EntityFrameworkCore;

namespace BirthdayReminder.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task DeleteUserAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Remove(user);
        await Task.CompletedTask;
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<User>> GetDueUsersAsync(DateTime utcNow, int batchSize, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Where(x => x.BirthdayNotificationUtc <= utcNow)
            .OrderBy(x => x.BirthdayNotificationUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}