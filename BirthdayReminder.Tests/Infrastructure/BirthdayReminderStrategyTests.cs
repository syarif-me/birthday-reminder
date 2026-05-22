using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Application.Services;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;
using BirthdayReminder.Infrastructure.Services;
using Moq;
using static BirthdayReminder.Tests.TestFactories;

namespace BirthdayReminder.Tests.Infrastructure;

public class BirthdayReminderStrategyTests
{
    private readonly Mock<IEmailReminder> _emailReminderMock;
    private readonly TimeZoneService _timeZoneService;
    private readonly Mock<IReminderRepository> _reminderRepositoryMock;
    private readonly BirthdayReminderStrategy _strategy;

    public BirthdayReminderStrategyTests()
    {
        _emailReminderMock = new Mock<IEmailReminder>();
        _timeZoneService = new TimeZoneService();
        _reminderRepositoryMock = new Mock<IReminderRepository>();
        _strategy = new BirthdayReminderStrategy(
            _emailReminderMock.Object,
            _timeZoneService,
            _reminderRepositoryMock.Object
        );
    }

    [Fact]
    public void Type_ShouldBeBirthday()
    {
        Assert.Equal(ReminderType.Birthday, _strategy.Type);
    }

    [Fact]
    public async Task ProcessReminderAsync_ShouldSkip_WhenReminderIsAlreadySent()
    {
        var user = CreateUser();
        var scheduledDate = new DateOnly(DateTime.UtcNow.Year, 5, 21);

        var sentReminder = new Reminder(user.Id, scheduledDate, ReminderType.Birthday);
        sentReminder.MarkAsSent(DateTime.UtcNow);

        _reminderRepositoryMock
            .Setup(repo => repo.GetReminderAsync(user.Id, ReminderType.Birthday, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentReminder);

        await _strategy.ProcessReminderAsync(user, CancellationToken.None);

        _emailReminderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _reminderRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReminderAsync_ShouldSkipAndReschedule_WhenReminderMaxFailedRetriesReached()
    {
        var user = CreateUser();
        var scheduledDate = new DateOnly(DateTime.UtcNow.Year, 5, 21);

        var failedReminder = new Reminder(user.Id, scheduledDate, ReminderType.Birthday);
        failedReminder.MarkAsFailed("Fail 1");
        failedReminder.MarkAsFailed("Fail 2");
        failedReminder.MarkAsFailed("Fail 3");

        _reminderRepositoryMock
            .Setup(repo => repo.GetReminderAsync(user.Id, ReminderType.Birthday, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedReminder);

        _reminderRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var originalNotificationUtc = user.BirthdayNotificationUtc;

        await _strategy.ProcessReminderAsync(user, CancellationToken.None);

        _emailReminderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.True(user.BirthdayNotificationUtc > originalNotificationUtc);
        _reminderRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReminderAsync_ShouldCreateAndSendReminder_WhenNew()
    {
        var today = DateTime.UtcNow;
        var user = CreateUser(birthday: new DateOnly(1995, today.Month, today.Day), notificationUtc: today);

        _reminderRepositoryMock
            .Setup(repo => repo.GetReminderAsync(user.Id, ReminderType.Birthday, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reminder?)null);

        Reminder? capturedReminder = null;
        _reminderRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Reminder>(), It.IsAny<CancellationToken>()))
            .Callback<Reminder, CancellationToken>((r, _) => capturedReminder = r)
            .Returns(Task.CompletedTask);

        _emailReminderMock
            .Setup(x => x.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _reminderRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var originalNotificationUtc = user.BirthdayNotificationUtc;

        await _strategy.ProcessReminderAsync(user, CancellationToken.None);

        Assert.NotNull(capturedReminder);
        Assert.Equal(ReminderStatus.Sent, capturedReminder.Status);
        Assert.Equal(1, capturedReminder.SentAtUtc.HasValue ? 1 : 0);

        _emailReminderMock.Verify(x => x.SendEmailAsync(user.Email, $"Hey, {user.FullName} it’s your birthday", It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(user.BirthdayNotificationUtc > originalNotificationUtc);
        _reminderRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReminderAsync_ShouldIncrementRetryCount_WhenEmailThrowsTransientError()
    {
        var today = DateTime.UtcNow;
        var user = CreateUser(birthday: new DateOnly(1995, today.Month, today.Day), notificationUtc: today);
        var scheduledDate = new DateOnly(today.Year, today.Month, today.Day);

        var reminder = new Reminder(user.Id, scheduledDate, ReminderType.Birthday);

        _reminderRepositoryMock
            .Setup(repo => repo.GetReminderAsync(user.Id, ReminderType.Birthday, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reminder);

        var exceptionMessage = "Network timed out.";
        _emailReminderMock
            .Setup(x => x.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(exceptionMessage));

        _reminderRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _strategy.ProcessReminderAsync(user, CancellationToken.None));
        Assert.Equal(exceptionMessage, ex.Message);

        Assert.Equal(ReminderStatus.Failed, reminder.Status);
        Assert.Equal(exceptionMessage, reminder.ErrorMessage);
        Assert.Equal(1, reminder.RetryCount);

        _reminderRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReminderAsync_ShouldReschedule_WhenEmailThrowsAndMaxRetryIsReached()
    {
        var today = DateTime.UtcNow;
        var user = CreateUser(birthday: new DateOnly(1995, today.Month, today.Day), notificationUtc: today);
        var scheduledDate = new DateOnly(today.Year, today.Month, today.Day);

        var reminder = new Reminder(user.Id, scheduledDate, ReminderType.Birthday);
        reminder.MarkAsFailed("Fail 1");
        reminder.MarkAsFailed("Fail 2");

        _reminderRepositoryMock
            .Setup(repo => repo.GetReminderAsync(user.Id, ReminderType.Birthday, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reminder);

        var exceptionMessage = "API limits exceeded.";
        _emailReminderMock
            .Setup(x => x.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(exceptionMessage));

        _reminderRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var originalNotificationUtc = user.BirthdayNotificationUtc;

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _strategy.ProcessReminderAsync(user, CancellationToken.None));
        Assert.Equal(exceptionMessage, ex.Message);

        Assert.Equal(ReminderStatus.Failed, reminder.Status);
        Assert.Equal(exceptionMessage, reminder.ErrorMessage);
        Assert.Equal(3, reminder.RetryCount);

        Assert.True(user.BirthdayNotificationUtc > originalNotificationUtc);
        _reminderRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReminderAsync_ShouldHandleLeapYearBirthdays_OnNonLeapYears()
    {
        var user = CreateUser(birthday: new DateOnly(1996, 2, 29), timeZone: "UTC");

        DateOnly? capturedDate = null;
        _reminderRepositoryMock
            .Setup(repo => repo.GetReminderAsync(user.Id, ReminderType.Birthday, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, ReminderType, DateOnly, CancellationToken>((_, _, d, _) => capturedDate = d)
            .ReturnsAsync((Reminder?)null);

        _reminderRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Reminder>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailReminderMock
            .Setup(x => x.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _reminderRepositoryMock
            .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _strategy.ProcessReminderAsync(user, CancellationToken.None);

        Assert.NotNull(capturedDate);

        var expectedDay = DateTime.IsLeapYear(DateTime.UtcNow.Year) ? 29 : 28;
        Assert.Equal(expectedDay, capturedDate.Value.Day);
        Assert.Equal(2, capturedDate.Value.Month);
    }
}
