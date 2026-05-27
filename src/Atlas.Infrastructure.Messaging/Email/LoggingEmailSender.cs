using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Email;

/// <summary>
/// Adapter d'email de DÉVELOPPEMENT : écrit le lien de vérification dans les logs au lieu d'envoyer un email.
/// À remplacer par l'adapter Brevo (provider France RGPD-compliant) avant toute mise en production.
/// </summary>
internal sealed class LoggingEmailSender(
    ILogger<LoggingEmailSender> logger,
    IOptions<EmailOptions> options) : IEmailSender
{
    public Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            string link = $"{options.Value.VerificationBaseUrl}" +
                $"?userId={userId.Value}&token={Uri.EscapeDataString(verificationToken)}";

            logger.LogInformation(
                "[DEV] Email de vérification pour {Recipient}. Lien de vérification : {VerificationLink}",
                recipient.Value,
                link);
        }

        return Task.CompletedTask;
    }
}
