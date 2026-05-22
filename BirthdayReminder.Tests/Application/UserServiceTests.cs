using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Application.Services;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Exceptions;
using Moq;
using static BirthdayReminder.Tests.TestFactories;

namespace BirthdayReminder.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly TimeZoneService _timeZoneService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _timeZoneService = new TimeZoneService();
        _userService = new UserService(_userRepositoryMock.Object, _timeZoneService);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCalculateNotificationUtcAndSaveUser()
    {
        var request = CreateCreateUserRequest();

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _userService.CreateUserAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(request.FirstName, result.FirstName);

        _userRepositoryMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(capturedUser);
        Assert.Equal(result, capturedUser);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldModifyUserAndSave_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var existingUser = CreateUser();

        _userRepositoryMock
            .Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var updateRequest = CreateUpdateUserRequest();

        await _userService.UpdateUserAsync(userId, updateRequest, CancellationToken.None);

        Assert.Equal(updateRequest.FirstName, existingUser.FirstName);

        _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var updateRequest = CreateUpdateUserRequest();

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _userService.UpdateUserAsync(userId, updateRequest, CancellationToken.None));

        _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldRemoveUserAndSave_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var existingUser = CreateUser();

        _userRepositoryMock
            .Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(repo => repo.DeleteUserAsync(existingUser, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _userService.DeleteUserAsync(userId, CancellationToken.None);

        _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _userService.DeleteUserAsync(userId, CancellationToken.None));

        _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDueUsersAsync_ShouldCallRepositoryWithCorrectParams()
    {
        var utcNow = DateTime.UtcNow;
        var batchSize = 100;
        var mockUsers = new List<User>
        {
            CreateUser()
        };

        _userRepositoryMock
            .Setup(repo => repo.GetDueUsersAsync(utcNow, batchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUsers);

        var result = await _userService.GetDueUsersAsync(utcNow, batchSize, CancellationToken.None);

        Assert.Equal(mockUsers, result);
        _userRepositoryMock.Verify(repo => repo.GetDueUsersAsync(utcNow, batchSize, It.IsAny<CancellationToken>()), Times.Once);
    }
}
