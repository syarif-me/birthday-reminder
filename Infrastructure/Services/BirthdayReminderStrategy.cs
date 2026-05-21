using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Application.Services;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;

namespace BirthdayReminder.Infrastructure.Services;

public class BirthdayReminderStrategy(
    IEmailReminder emailReminder,
    TimeZoneService timeZoneService,
    IReminderRepository reminderRepository) : IReminderStrategy
{
    public ReminderType Type => ReminderType.Birthday;

    public async Task ProcessReminderAsync(User user, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var year = today.Year;
        var month = user.Birthday.Month;
        var day = user.Birthday.Day;

        if (month == 2 && day == 29 && !DateTime.IsLeapYear(year))
        {
            day = 28;
        }

        var scheduledDate = new DateOnly(year, month, day);

        var reminder = await reminderRepository.GetReminderAsync(user.Id, ReminderType.Birthday, scheduledDate, cancellationToken);
        if (reminder?.Status == ReminderStatus.Sent) return;

        if (reminder?.Status == ReminderStatus.Failed && reminder.RetryCount >= 3)
        {
            var nextBirthdayNotificationUtc = timeZoneService.CalculateBirthdayNotificationUtc(
                user.Birthday,
                user.TimeZone);
            user.SetNextBirthdayNotificationUtc(nextBirthdayNotificationUtc);
            await reminderRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        if (reminder == null)
        {
            reminder = new Reminder(user.Id, scheduledDate, ReminderType.Birthday);
            await reminderRepository.AddAsync(reminder, cancellationToken);
        }

        try
        {
            var emailBody = $"Hey, {user.FullName} it’s your birthday";
            await emailReminder.SendEmailAsync(
                user.Email,
                emailBody,
                cancellationToken);
            reminder.MarkAsSent(DateTime.UtcNow);

            var nextBirthdayNotificationUtc = timeZoneService.CalculateBirthdayNotificationUtc(
                user.Birthday,
                user.TimeZone);
            user.SetNextBirthdayNotificationUtc(nextBirthdayNotificationUtc);
        }
        catch (Exception ex)
        {
            reminder.MarkAsFailed(ex.Message);

            if (reminder.RetryCount >= 3)
            {
                var nextBirthdayNotificationUtc = timeZoneService.CalculateBirthdayNotificationUtc(
                    user.Birthday,
                    user.TimeZone);
                user.SetNextBirthdayNotificationUtc(nextBirthdayNotificationUtc);
            }
            throw;
        }
        finally
        {
            await reminderRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
