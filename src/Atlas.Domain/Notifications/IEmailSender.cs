using Atlas.Domain.Users;

namespace Atlas.Domain.Notifications;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default);
}
