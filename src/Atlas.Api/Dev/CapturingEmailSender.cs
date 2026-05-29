using Atlas.Domain.Favorites;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;

namespace Atlas.Api.Dev;

/// <summary>
/// DÉVELOPPEMENT UNIQUEMENT. Reproduit l'envoi d'email de dev (lien écrit dans les logs) mais capture
/// en plus le token dans <see cref="DevVerificationTokenStore"/> pour l'endpoint /dev/verification-token.
/// Remplace <c>LoggingEmailSender</c> uniquement en environnement Development.
/// </summary>
internal sealed class CapturingEmailSender(
    ILogger<CapturingEmailSender> logger,
    DevVerificationTokenStore store,
    IConfiguration configuration) : IEmailSender
{
    public Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default)
    {
        store.Record(recipient.Value, userId.Value, verificationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            string baseUrl = configuration["Email:VerificationBaseUrl"] ?? "(non configuré)";
            string link = $"{baseUrl}?userId={userId.Value}&token={Uri.EscapeDataString(verificationToken)}";
            logger.LogInformation(
                "[DEV] Email de vérification pour {Recipient}. Lien de vérification : {VerificationLink}",
                recipient.Value,
                link);
        }

        return Task.CompletedTask;
    }

    public Task SendFavoriteChangeAsync(
        EmailAddress recipient,
        string sirenValue,
        string? denomination,
        IReadOnlyList<CompanyFavoriteChange> changes,
        CancellationToken ct = default)
    {
        // En dev on log seulement — pas de capture nécessaire pour les tests automatisés.
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[DEV] Alerte favori pour {Recipient} : {Denomination} ({Siren}) — {ChangeCount} changement(s).",
                recipient.Value,
                denomination ?? "(sans dénomination)",
                sirenValue,
                changes.Count);
        }
        return Task.CompletedTask;
    }

    public Task SendFeedRuleMatchedAsync(
        EmailAddress recipient,
        string ruleName,
        IReadOnlyList<FeedRuleMatch> matches,
        CancellationToken ct = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[DEV] Règle de veille {RuleName} pour {Recipient} — {MatchCount} match(s).",
                ruleName,
                recipient.Value,
                matches.Count);
        }
        return Task.CompletedTask;
    }
}
