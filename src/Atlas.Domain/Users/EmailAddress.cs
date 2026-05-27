using System.Net.Mail;
using Atlas.Shared.Result;

namespace Atlas.Domain.Users;

public readonly record struct EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result<EmailAddress>.Fail(UserErrors.InvalidEmail(raw ?? string.Empty));
        }

        string trimmed = raw.Trim();
        if (trimmed.Length > 254
            || !MailAddress.TryCreate(trimmed, out MailAddress? parsed)
            || parsed is null)
        {
            return Result<EmailAddress>.Fail(UserErrors.InvalidEmail(trimmed));
        }

        return Result<EmailAddress>.Ok(new EmailAddress(parsed.Address.ToLowerInvariant()));
    }

    /// <summary>
    /// Réhydrate une adresse depuis une valeur déjà validée et persistée. Réservé à la couche persistance.
    /// </summary>
    internal static EmailAddress FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
