using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Domain.Entities;
using BirthdayReminder.Domain.Enums;
using BirthdayReminder.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static BirthdayReminder.Tests.TestFactories;

namespace BirthdayReminder.Tests.Workers;

public class ReminderWorkerTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IReminderStrategy> _strategyMock;
    private readonly Mock<ILogger<ReminderWorker>> _loggerMock;

    public ReminderWorkerTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _strategyMock = new Mock<IReminderStrategy>();
        _loggerMock = new Mock<ILogger<ReminderWorker>>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _strategyMock.Setup(x => x.Type).Returns(ReminderType.Birthday);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IUserRepository)))
            .Returns(_userRepositoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IReminderStrategy>)))
            .Returns(new List<IReminderStrategy> { _strategyMock.Object });

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceProvider)))
            .Returns(_serviceProviderMock.Object);

        _loggerMock
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ))
            .Callback(new InvocationAction(invocation =>
            {
                if (invocation.Arguments[3] is Exception exception)
                {
                    Console.WriteLine($"ReminderWorker Exception: {exception}");
                }
            }));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessDueUsersAndExit_OnCancellation()
    {
        var cts = new CancellationTokenSource();
        var dueUsers = new List<User>
        {
            CreateUser()
        };

        _userRepositoryMock
            .Setup(x => x.GetDueUsersAsync(It.IsAny<DateTime>(), 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dueUsers)
            .Callback(() => cts.Cancel());

        _strategyMock
            .Setup(x => x.ProcessReminderAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var worker = new ReminderWorker(_scopeFactoryMock.Object, _loggerMock.Object);

        await worker.StartAsync(cts.Token);
        await Task.Delay(50);
        await worker.StopAsync(CancellationToken.None);

        _userRepositoryMock.Verify(x => x.GetDueUsersAsync(It.IsAny<DateTime>(), 100, It.IsAny<CancellationToken>()), Times.Once);
        _strategyMock.Verify(x => x.ProcessReminderAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLoopImmediately_WhenBatchSizeIsExactly100()
    {
        var cts = new CancellationTokenSource();

        var dueUsers = new List<User>();
        for (int i = 0; i < 100; i++)
        {
            dueUsers.Add(CreateUser(firstName: $"User{i}", lastName: "LastName", email: $"user{i}@email.com", birthday: new DateOnly(1990, 1, 1), timeZone: "UTC"));
        }

        var callCount = 0;
        _userRepositoryMock
            .Setup(x => x.GetDueUsersAsync(It.IsAny<DateTime>(), 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 2)
                {
                    cts.Cancel();
                    return [];
                }
                return dueUsers;
            });

        _strategyMock
            .Setup(x => x.ProcessReminderAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var worker = new ReminderWorker(_scopeFactoryMock.Object, _loggerMock.Object);

        await worker.StartAsync(cts.Token);
        await Task.Delay(50);
        await worker.StopAsync(CancellationToken.None);

        Assert.Equal(2, callCount);
        _userRepositoryMock.Verify(x => x.GetDueUsersAsync(It.IsAny<DateTime>(), 100, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _strategyMock.Verify(x => x.ProcessReminderAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Exactly(100));
    }
}
