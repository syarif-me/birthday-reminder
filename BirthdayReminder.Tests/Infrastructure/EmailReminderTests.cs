using System.Net;
using System.Net.Http.Json;
using BirthdayReminder.Infrastructure.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace BirthdayReminder.Tests.Infrastructure;

public class EmailReminderTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldSendCorrectPayloadAndSucceed_OnImmediateSuccess()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"sent\"}")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://email-service.digitalenvision.com.au/")
        };

        var emailReminder = new EmailReminder(httpClient);
        var recipient = "test@email.com";
        var message = "Test";

        await emailReminder.SendEmailAsync(recipient, message, CancellationToken.None);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("https://email-service.digitalenvision.com.au/send-email")
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SendEmailAsync_ShouldRetryAndSucceed_OnTransientFailureFollowedBySuccess()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var callCount = 0;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Test connection failure.");
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"status\":\"sent\"}")
                };
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://email-service.digitalenvision.com.au/")
        };

        var emailReminder = new EmailReminder(httpClient);

        await emailReminder.SendEmailAsync("test@email.com", "Test", CancellationToken.None);

        Assert.Equal(2, callCount);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
