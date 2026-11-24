using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OneStep.Application.Abstractions;

namespace OneStep.Infrastructure.Email;

/// <summary>
/// Development "email": instead of sending, writes the email to the log (console and logs/ file).
/// A diploma project has no mail server, so this keeps password reset testable.
/// In production you would register an SMTP or SendGrid sender instead, in DependencyInjection.cs.
/// </summary>
public class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _logger;
    private readonly string _frontendUrl;

    public LogEmailSender(ILogger<LogEmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _frontendUrl = (configuration["Frontend:Url"] ?? "http://localhost:4200").TrimEnd('/');
    }

    public Task SendPasswordResetAsync(string toEmail, string firstName, string resetToken, CancellationToken cancellationToken)
    {
        var link = $"{_frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        _logger.LogWarning(
            "EMAIL (development) to {Email}: Hi {FirstName}, open this link to choose a new password " +
            "(valid for 1 hour): {ResetLink}",
            toEmail, firstName, link);

        return Task.CompletedTask;
    }
}
