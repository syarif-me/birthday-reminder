namespace BirthdayReminder.Domain.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly GetNotificationDate(this DateOnly birthday, int year)
    {
        var month = birthday.Month;
        var day = birthday.Day;

        if (month == 2 && day == 29 && !DateTime.IsLeapYear(year))
        {
            day = 28;
        }

        return new DateOnly(year, month, day);
    }
}
