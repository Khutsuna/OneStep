namespace OneStep.Application.Abstractions;

/// <summary>
/// Sends emails to users. The Application layer only knows this interface;
/// Infrastructure decides HOW the email goes out. Today: written to the log
/// (LogEmailSender). In production: an SMTP or SendGrid implementation,
/// registered in DependencyInjection.cs, with no change to the business logic.
/// </summary>
public interface IEmailSender
{
    /// <summary>Sends the "reset your password" email with a link that contains the token.</summary>
    Task SendPasswordResetAsync(string toEmail, string firstName, string resetToken, CancellationToken cancellationToken);
}
