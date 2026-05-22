using BirthdayReminder.Domain.Extensions;
using NodaTime;
using NodaTime.TimeZones;

namespace BirthdayReminder.Application.Services;

public class TimeZoneService
{
    public DateTime CalculateBirthdayNotificationUtc(DateOnly birthday, string timeZone)
    {
        var zone = DateTimeZoneProviders.Tzdb[timeZone];
        var now = SystemClock.Instance.GetCurrentInstant();
        var currentYear = now.InZone(zone).Year;

        var localDateTime = GetNotificationLocalDateTime(birthday, currentYear);

        if (zone.AtLeniently(localDateTime).ToInstant() < now)
        {
            localDateTime = GetNotificationLocalDateTime(birthday, currentYear + 1);
        }

        return zone.AtLeniently(localDateTime).ToDateTimeUtc();
    }

    private static LocalDateTime GetNotificationLocalDateTime(DateOnly birthday, int year)
    {
        var date = birthday.GetNotificationDate(year);
        return new LocalDateTime(date.Year, date.Month, date.Day, 9, 0, 0);
    }
}