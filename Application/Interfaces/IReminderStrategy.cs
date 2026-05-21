using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;

namespace BirthdayReminder.Application.Interfaces;

public interface IReminderStrategy
{
    ReminderType Type { get; }
    Task ProcessReminderAsync(User user, CancellationToken cancellationToken);
}
