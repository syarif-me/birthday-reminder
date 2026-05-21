namespace BirthdayReminder.Application.Interfaces;

public interface IEmailReminder
{
    Task SendEmailAsync(string to, string message, CancellationToken cancellationToken);
}