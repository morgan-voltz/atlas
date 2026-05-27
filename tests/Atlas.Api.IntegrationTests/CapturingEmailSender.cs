using Atlas.Domain.Notifications;
using Atlas.Domain.Users;

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
}
