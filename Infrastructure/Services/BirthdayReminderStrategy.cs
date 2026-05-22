using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Application.Services;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;
using BirthdayReminder.Domain.Extensions;

namespace BirthdayReminder.Infrastructure.Services;

public class BirthdayReminderStrategy(
    IEmailReminder emailReminder,
    TimeZoneService timeZoneService,
    IReminderRepository reminderRepository) : IReminderStrategy
{
    public ReminderType Type => ReminderType.Birthday;

    public async Task ProcessReminderAsync(User user, CancellationToken cancellationToken)
    {
        var scheduledDate = user.Birthday.GetNotificationDate(DateTime.UtcNow.Year);

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
