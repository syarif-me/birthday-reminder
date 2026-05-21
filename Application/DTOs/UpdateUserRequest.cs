namespace BirthdayReminder.Application.DTOs;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    DateOnly Birthday,
    string TimeZone
);