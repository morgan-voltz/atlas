using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using Atlas.Domain.Favorites;
using Atlas.Domain.Notifications;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.Email.Brevo;

/// <summary>
/// Adapter Brevo de <see cref="IEmailSender"/> (provider France RGPD-compliant, cf. CLAUDE.md).
/// Envoie les emails transactionnels via <c>POST /v3/smtp/email</c> avec authentification par
/// header <c>api-key</c>. Tout échec d'envoi est loggé mais ne lève jamais : les emails ne
/// doivent pas casser les flux métier (refresh favoris F-019, évaluation règles F-046, …).
/// </summary>
internal sealed class BrevoEmailSender(
    HttpClient httpClient,
    IOptions<EmailOptions> emailOptions,
    IOptions<BrevoOptions> brevoOptions,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private const string SendEndpoint = "/v3/smtp/email";

    private readonly EmailOptions _email = emailOptions.Value;
    private readonly BrevoOptions _brevo = brevoOptions.Value;

    public Task SendEmailVerificationAsync(
        EmailAddress recipient,
        UserId userId,
        string verificationToken,
        CancellationToken ct = default)
    {
        string link = $"{_email.VerificationBaseUrl}?userId={userId.Value}&token={Uri.EscapeDataString(verificationToken)}";

        string subject = "Confirmez votre adresse Atlas";
        string text =
            $"Bonjour,\n\nCliquez sur le lien suivant pour confirmer votre adresse Atlas :\n{link}\n\n" +
            "Si vous n'êtes pas à l'origine de cette inscription, ignorez cet email.\n\n— L'équipe Atlas";

        string html =
            $"""
            <p>Bonjour,</p>
            <p>Cliquez sur le lien suivant pour confirmer votre adresse Atlas :</p>
            <p><a href="{HttpUtility.HtmlAttributeEncode(link)}">Confirmer mon adresse</a></p>
            <p>Si vous n'êtes pas à l'origine de cette inscription, ignorez cet email.</p>
            <p>— L'équipe Atlas</p>
            """;

        return SendAsync(recipient, subject, html, text, "email-verification", ct);
    }

    public Task SendFavoriteChangeAsync(
        EmailAddress recipient,
        string sirenValue,
        string? denomination,
        IReadOnlyList<CompanyFavoriteChange> changes,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(changes);

        string label = string.IsNullOrWhiteSpace(denomination) ? sirenValue : denomination!;
        string subject = $"Atlas — {label} a évolué";

        var textBuilder = new StringBuilder();
        textBuilder.Append("Bonjour,\n\n");
        textBuilder.Append(CultureInfo.InvariantCulture, $"Votre favori « {label} » (SIREN {sirenValue}) a évolué :\n\n");
        foreach (CompanyFavoriteChange change in changes)
        {
            textBuilder.Append(CultureInfo.InvariantCulture, $"- {change.Field} : {change.OldValue ?? "—"} → {change.NewValue ?? "—"}\n");
        }
        textBuilder.Append("\n— L'équipe Atlas");

        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<p>Bonjour,</p>");
        htmlBuilder.Append(CultureInfo.InvariantCulture,
            $"<p>Votre favori <strong>{HttpUtility.HtmlEncode(label)}</strong> (SIREN {sirenValue}) a évolué :</p>");
        htmlBuilder.Append("<ul>");
        foreach (CompanyFavoriteChange change in changes)
        {
            htmlBuilder.Append(CultureInfo.InvariantCulture,
                $"<li><strong>{HttpUtility.HtmlEncode(change.Field)}</strong> : {HttpUtility.HtmlEncode(change.OldValue ?? "—")} → {HttpUtility.HtmlEncode(change.NewValue ?? "—")}</li>");
        }
        htmlBuilder.Append("</ul>");
        htmlBuilder.Append("<p>— L'équipe Atlas</p>");

        return SendAsync(recipient, subject, htmlBuilder.ToString(), textBuilder.ToString(), "favorite-change", ct);
    }

    public Task SendFeedRuleMatchedAsync(
        EmailAddress recipient,
        string ruleName,
        IReadOnlyList<FeedRuleMatch> matches,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(matches);

        string subject = matches.Count == 1
            ? $"Atlas — Règle « {ruleName} » : 1 nouvel item"
            : $"Atlas — Règle « {ruleName} » : {matches.Count.ToString(CultureInfo.InvariantCulture)} nouveaux items";

        var textBuilder = new StringBuilder();
        textBuilder.Append("Bonjour,\n\n");
        textBuilder.Append(CultureInfo.InvariantCulture,
            $"Votre règle de surveillance « {ruleName} » a matché {matches.Count.ToString(CultureInfo.InvariantCulture)} nouvel(s) item(s) :\n\n");
        foreach (FeedRuleMatch match in matches)
        {
            string when = match.PublishedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string url = string.IsNullOrWhiteSpace(match.Url) ? string.Empty : $" — {match.Url}";
            textBuilder.Append(CultureInfo.InvariantCulture,
                $"- [{when}] {match.Title} ({match.SourceName}){url}\n");
        }
        textBuilder.Append("\n— L'équipe Atlas");

        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<p>Bonjour,</p>");
        htmlBuilder.Append(CultureInfo.InvariantCulture,
            $"<p>Votre règle de surveillance <strong>{HttpUtility.HtmlEncode(ruleName)}</strong> a matché {matches.Count.ToString(CultureInfo.InvariantCulture)} nouvel(s) item(s) :</p>");
        htmlBuilder.Append("<ul>");
        foreach (FeedRuleMatch match in matches)
        {
            string when = match.PublishedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string title = HttpUtility.HtmlEncode(match.Title);
            string sourceName = HttpUtility.HtmlEncode(match.SourceName);
            if (string.IsNullOrWhiteSpace(match.Url))
            {
                htmlBuilder.Append(CultureInfo.InvariantCulture,
                    $"<li><strong>{title}</strong> — {sourceName} ({when})</li>");
            }
            else
            {
                string href = HttpUtility.HtmlAttributeEncode(match.Url);
                htmlBuilder.Append(CultureInfo.InvariantCulture,
                    $"<li><a href=\"{href}\">{title}</a> — {sourceName} ({when})</li>");
            }
        }
        htmlBuilder.Append("</ul>");
        htmlBuilder.Append("<p>— L'équipe Atlas</p>");

        return SendAsync(recipient, subject, htmlBuilder.ToString(), textBuilder.ToString(), "feed-rule-matched", ct);
    }

    private async Task SendAsync(
        EmailAddress recipient,
        string subject,
        string htmlContent,
        string textContent,
        string templateTag,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_brevo.SenderEmail))
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Brevo : SenderEmail manquant, envoi {Template} abandonné.", templateTag);
            }
            return;
        }

        var payload = new BrevoSendPayload(
            new BrevoContact(_brevo.SenderEmail!, _brevo.SenderName),
            [new BrevoContact(recipient.Value, null)],
            subject,
            htmlContent,
            textContent);

        using var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
        {
            Content = JsonContent.Create(payload),
        };

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Brevo : email {Template} envoyé à {Recipient} ({Status}).",
                        templateTag,
                        recipient.Value,
                        (int)response.StatusCode);
                }
                return;
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                string responseBody = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning(
                    "Brevo : envoi {Template} en échec ({Status}) — {Body}",
                    templateTag,
                    (int)response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(ex, "Brevo : exception réseau lors de l'envoi {Template}.", templateTag);
            }
        }
    }

    private sealed record BrevoContact(string Email, string? Name);

    private sealed record BrevoSendPayload(
        BrevoContact Sender,
        IReadOnlyList<BrevoContact> To,
        string Subject,
        string HtmlContent,
        string TextContent);
}
