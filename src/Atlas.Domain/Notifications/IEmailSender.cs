using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;

namespace Atlas.Domain.Notifications;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default);

    /// <summary>Lien de réinitialisation de mot de passe (clair, à usage unique). Le token n'est jamais persisté en clair.</summary>
    Task SendPasswordResetAsync(
        EmailAddress recipient,
        UserId userId,
        string resetToken,
        CancellationToken ct = default);

    /// <summary>Alerte quotidienne F-019 : un favori d'entreprise a évolué (changement de dénomination, dirigeants, adresse, …).</summary>
    Task SendFavoriteChangeAsync(
        EmailAddress recipient,
        string sirenValue,
        string? denomination,
        IReadOnlyList<CompanyFavoriteChange> changes,
        CancellationToken ct = default);

    /// <summary>Alerte F-046 : une règle de surveillance personnalisée a matché un ou plusieurs nouveaux items de veille.</summary>
    Task SendFeedRuleMatchedAsync(
        EmailAddress recipient,
        string ruleName,
        IReadOnlyList<FeedRuleMatch> matches,
        CancellationToken ct = default);
}
