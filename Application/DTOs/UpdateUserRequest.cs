namespace BirthdayReminder.Application.DTOs;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    DateOnly Birthday,
    string TimeZone
);