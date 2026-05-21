using System.Net.Http.Json;
using BirthdayReminder.Application.Interfaces;
using Polly;

namespace BirthdayReminder.Infrastructure.Services;

public class EmailReminder(HttpClient httpClient) : IEmailReminder
{
    public async Task SendEmailAsync(string to, string message, CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ]);

        var payload = new
        {
            email = to,
            message
        };

        await retryPolicy.ExecuteAsync(async () =>
        {
            var response = await httpClient.PostAsJsonAsync(
                "send-email",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        });
    }
}
