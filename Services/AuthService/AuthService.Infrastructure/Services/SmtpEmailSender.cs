using System.Net.Mail;
using System.Net;
using AuthService.Application.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var host = _config["Email:SmtpHost"];
        var portStr = _config["Email:SmtpPort"];
        var user = _config["Email:Username"];
        var pass = _config["Email:Password"];
        var from = _config["Email:From"] ?? user;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr))
        {
            _logger.LogWarning("[SmtpEmailSender] SMTP not configured, falling back to console log. To={ToEmail}", toEmail);
            Console.WriteLine($"[Email fallback] To: {toEmail} | Subject: {subject} | Body: {body}");
            return;
        }

        var port = int.Parse(portStr);
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };
        var mail = new MailMessage(from!, toEmail, subject, body) { IsBodyHtml = false };
        await client.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("[SmtpEmailSender] Email sent to {ToEmail}", toEmail);
    }
}
