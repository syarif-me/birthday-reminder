namespace BirthdayReminder.Application.DTOs;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    DateOnly Birthday,
    string TimeZone
);