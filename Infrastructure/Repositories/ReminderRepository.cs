using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;
using BirthdayReminder.Infrastructure.Persistences;
using Microsoft.EntityFrameworkCore;

namespace BirthdayReminder.Infrastructure.Repositories;

public class ReminderRepository(AppDbContext dbContext) : IReminderRepository
{
    public async Task<bool> ExistsAsync(Guid userId, ReminderType type, DateOnly scheduledDate, ReminderStatus status, CancellationToken cancellationToken)
    {
        return await dbContext.Reminders.AnyAsync(r =>
            r.UserId == userId &&
            r.Type == type &&
            r.ScheduledDate == scheduledDate &&
            r.Status == status,
            cancellationToken);
    }

    public async Task AddAsync(Reminder reminder, CancellationToken cancellationToken)
    {
        await dbContext.Reminders.AddAsync(reminder, cancellationToken);
    }

    public async Task<Reminder?> GetReminderAsync(Guid userId, ReminderType type, DateOnly scheduledDate, CancellationToken cancellationToken)
    {
        return await dbContext.Reminders.FirstOrDefaultAsync(r =>
            r.UserId == userId &&
            r.Type == type &&
            r.ScheduledDate == scheduledDate,
            cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
