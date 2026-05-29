using Atlas.Domain.Favorites;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;

namespace Atlas.Api.IntegrationTests;

/// <summary>
/// Double de test de <see cref="IEmailSender"/> : capture le dernier token de vérification émis,
/// afin que le test e2e puisse appeler /auth/verify-email (le token n'est pas exposé par l'API).
/// </summary>
public sealed class CapturingEmailSender : IEmailSender
{
    public Guid LastUserId { get; private set; }

    public string? LastVerificationToken { get; private set; }

    public Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default)
    {
        LastUserId = userId.Value;
        LastVerificationToken = verificationToken;
        return Task.CompletedTask;
    }

    public Task SendFavoriteChangeAsync(
        EmailAddress recipient,
        string sirenValue,
        string? denomination,
        IReadOnlyList<CompanyFavoriteChange> changes,
        CancellationToken ct = default) => Task.CompletedTask;

    public Task SendFeedRuleMatchedAsync(
        EmailAddress recipient,
        string ruleName,
        IReadOnlyList<FeedRuleMatch> matches,
        CancellationToken ct = default) => Task.CompletedTask;
}
