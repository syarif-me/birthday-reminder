using BirthdayReminder.Application.Interfaces;
using BirthdayReminder.Domain.Enums;

namespace BirthdayReminder.Workers;

public class ReminderWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool hasMoreUsers = false;

            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var strategies = scope.ServiceProvider.GetServices<IReminderStrategy>();

                var users = await repository.GetDueUsersAsync(DateTime.UtcNow, 100, stoppingToken);

                var birthdayStrategy = strategies.FirstOrDefault(s => s.Type == ReminderType.Birthday);
                if (birthdayStrategy == null)
                {
                    logger.LogWarning("Birthday reminder strategy is not registered.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                if (users.Count != 0)
                {
                    foreach (var user in users)
                    {
                        try
                        {
                            await birthdayStrategy.ProcessReminderAsync(user, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error sending birthday reminder to User {UserId}", user.Id);
                        }
                    }

                    hasMoreUsers = users.Count == 100;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in ReminderWorker");
            }

            if (!hasMoreUsers)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}