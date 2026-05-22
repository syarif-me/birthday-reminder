using BirthdayReminder.Application.DTOs;
using BirthdayReminder.Domain.Entities;

namespace BirthdayReminder.Tests;

public static class TestFactories
{
    public static User CreateUser(
        string firstName = "User",
        string lastName = "Test",
        string email = "test@email.com",
        DateOnly? birthday = null,
        string timeZone = "Asia/Jakarta",
        DateTime? notificationUtc = null)
    {
        return new User(
            firstName,
            lastName,
            email,
            birthday ?? new DateOnly(1995, 5, 21),
            timeZone,
            notificationUtc ?? DateTime.UtcNow
        );
    }

    public static CreateUserRequest CreateCreateUserRequest(
        string firstName = "User",
        string lastName = "Test",
        string email = "test@email.com",
        DateOnly? birthday = null,
        string timeZone = "Asia/Jakarta")
    {
        return new CreateUserRequest(
            firstName,
            lastName,
            email,
            birthday ?? new DateOnly(1995, 5, 21),
            timeZone
        );
    }

    public static UpdateUserRequest CreateUpdateUserRequest(
        string firstName = "Test",
        string lastName = "User",
        string email = "user@email.com",
        DateOnly? birthday = null,
        string timeZone = "Australia/Sydney")
    {
        return new UpdateUserRequest(
            firstName,
            lastName,
            email,
            birthday ?? new DateOnly(1996, 6, 20),
            timeZone
        );
    }
}
