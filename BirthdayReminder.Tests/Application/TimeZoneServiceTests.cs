using BirthdayReminder.Application.Services;

namespace BirthdayReminder.Tests.Application;

public class TimeZoneServiceTests
{
    private readonly TimeZoneService _timeZoneService = new();

    [Fact]
    public void CalculateBirthdayNotificationUtc_ShouldReturnCorrectUtcTime()
    {
        var birthday = new DateOnly(1990, 5, 21);
        var timeZone = "Asia/Jakarta";

        var result = _timeZoneService.CalculateBirthdayNotificationUtc(birthday, timeZone);

        Assert.Equal(2, result.Hour);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void CalculateBirthdayNotificationUtc_ShouldRollForwardIfTimeHasPassed()
    {
        var birthday = new DateOnly(1985, 1, 1);
        var timeZone = "UTC";

        var result = _timeZoneService.CalculateBirthdayNotificationUtc(birthday, timeZone);

        Assert.Equal(9, result.Hour);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void CalculateBirthdayNotificationUtc_ShouldHandleLeapYearBirthdays_OnNonLeapYears()
    {
        var birthday = new DateOnly(1996, 2, 29);
        var timeZone = "UTC";

        var result = _timeZoneService.CalculateBirthdayNotificationUtc(birthday, timeZone);

        Assert.Equal(9, result.Hour);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }
}
