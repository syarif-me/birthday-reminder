namespace BirthdayReminder.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateOnly Birthday { get; private set; }
    public string TimeZone { get; private set; } = default!;
    public DateTime BirthdayNotificationUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public User() { }

    public User(string firstName, string lastName, string email, DateOnly birthday, string timeZone, DateTime birthdayNotificationUtc)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Birthday = birthday;
        TimeZone = timeZone;
        BirthdayNotificationUtc = birthdayNotificationUtc;

        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void Update(string firstName, string lastName, string email, DateOnly birthday, string timeZone, DateTime birthdayNotificationUtc)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Birthday = birthday;
        TimeZone = timeZone;
        BirthdayNotificationUtc = birthdayNotificationUtc;

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetNextBirthdayNotificationUtc(DateTime birthdayNotificationUtc)
    {
        BirthdayNotificationUtc = birthdayNotificationUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}