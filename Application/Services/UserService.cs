using BirthdayReminder.Application.DTOs;
using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Exceptions;

namespace BirthdayReminder.Application.Services;

public class UserService(IUserRepository userRepository, TimeZoneService timeZoneService)
{
    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var birthdayNotificationUtc = timeZoneService.CalculateBirthdayNotificationUtc(request.Birthday, request.TimeZone);
        var user = new User(request.FirstName, request.LastName, request.Birthday, request.TimeZone, birthdayNotificationUtc);
        await userRepository.CreateUserAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);
        user.Update(request.FirstName, request.LastName, request.Birthday, request.TimeZone, timeZoneService.CalculateBirthdayNotificationUtc(request.Birthday, request.TimeZone));
        await userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByIdAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);
        await userRepository.DeleteUserAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<User>> GetDueUsersAsync(DateTime utcNow, int batchSize, CancellationToken cancellationToken = default)
    {
        return await userRepository.GetDueUsersAsync(utcNow, batchSize, cancellationToken);
    }
}