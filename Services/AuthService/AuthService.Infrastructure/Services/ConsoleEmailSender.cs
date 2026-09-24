using AuthService.Application.Services.IServices;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[ConsoleEmailSender] To={ToEmail} Subject={Subject} Body={Body}", toEmail, subject, body);
        Console.WriteLine($"[Email] To: {toEmail} | Subject: {subject} | Body: {body}");
        return Task.CompletedTask;
    }
}
