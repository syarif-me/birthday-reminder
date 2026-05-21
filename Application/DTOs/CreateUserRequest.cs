namespace BirthdayReminder.Application.DTOs;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    DateOnly Birthday,
    string TimeZone
);