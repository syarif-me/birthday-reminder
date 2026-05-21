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
        var localDateTime = new LocalDateTime(currentYear, birthday.Month, birthday.Day, 9, 0, 0);

        if (zone.AtLeniently(localDateTime).ToInstant() < now)
        {
            localDateTime = localDateTime.PlusYears(1);
        }

        return zone.AtLeniently(localDateTime).ToDateTimeUtc();
    }
}