using Atlas.Domain.Favorites;
using Atlas.Domain.Users;

namespace Atlas.Domain.Notifications;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default);

    /// <summary>Alerte quotidienne F-019 : un favori d'entreprise a évolué (changement de dénomination, dirigeants, adresse, …).</summary>
    Task SendFavoriteChangeAsync(
        EmailAddress recipient,
        string sirenValue,
        string? denomination,
        IReadOnlyList<CompanyFavoriteChange> changes,
        CancellationToken ct = default);
}
