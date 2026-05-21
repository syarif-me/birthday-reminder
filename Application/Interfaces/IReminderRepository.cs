using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;

namespace BirthdayReminder.Application.Interfaces;

public interface IReminderRepository
{
    Task<bool> ExistsAsync(Guid userId, ReminderType type, DateOnly scheduledDate, ReminderStatus status, CancellationToken cancellationToken);
    Task<Reminder?> GetReminderAsync(Guid userId, ReminderType type, DateOnly scheduledDate, CancellationToken cancellationToken);
    Task AddAsync(Reminder reminder, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
