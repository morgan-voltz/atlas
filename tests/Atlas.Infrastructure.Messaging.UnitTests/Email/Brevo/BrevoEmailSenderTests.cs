using System.Net;
using System.Text.Json;
using Atlas.Domain.Favorites;
using Atlas.Domain.Users;
using Atlas.Domain.Veille;
using Atlas.Infrastructure.Messaging.Email;
using Atlas.Infrastructure.Messaging.Email.Brevo;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.UnitTests.Email.Brevo;

public sealed class BrevoEmailSenderTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeHttpMessageHandler _handler = new();
    private readonly EmailAddress _recipient = EmailAddress.Create("user@example.test").Value!;

    public void Dispose() => _handler.Dispose();

    private BrevoEmailSender CreateSender(BrevoOptions? brevoOverride = null)
    {
        var httpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://api.brevo.test") };
        var email = Options.Create(new EmailOptions { VerificationBaseUrl = "https://atlas.test/verify" });
        var brevo = Options.Create(brevoOverride ?? new BrevoOptions
        {
            ApiKey = "xkeysib-test",
            SenderEmail = "noreply@atlas.test",
            SenderName = "Atlas",
        });
        return new BrevoEmailSender(httpClient, email, brevo, NullLogger<BrevoEmailSender>.Instance);
    }

    [Fact]
    public async Task SendEmailVerification_posts_expected_payload()
    {
        BrevoEmailSender sender = CreateSender();
        UserId userId = UserId.New();

        await sender.SendEmailVerificationAsync(_recipient, userId, "tok-abc", CancellationToken.None);

        _handler.Requests.Should().HaveCount(1);
        HttpRequestMessage request = _handler.Requests[0];
        request.RequestUri!.AbsolutePath.Should().Be("/v3/smtp/email");
        request.Method.Should().Be(HttpMethod.Post);

        string body = _handler.RequestBodies[0];
        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;
        root.GetProperty("sender").GetProperty("email").GetString().Should().Be("noreply@atlas.test");
        root.GetProperty("to")[0].GetProperty("email").GetString().Should().Be("user@example.test");
        root.GetProperty("subject").GetString().Should().Contain("Confirmez");
        root.GetProperty("htmlContent").GetString().Should().Contain("userId=" + userId.Value);
        root.GetProperty("htmlContent").GetString().Should().Contain("token=tok-abc");
        root.GetProperty("textContent").GetString().Should().Contain("verify");
    }

    [Fact]
    public async Task SendFavoriteChange_includes_each_change_in_body()
    {
        BrevoEmailSender sender = CreateSender();
        var changes = new List<CompanyFavoriteChange>
        {
            new("Denomination", "Old SAS", "New SAS"),
            new("Naf", "62.01Z", "63.99Z"),
        };

        await sender.SendFavoriteChangeAsync(_recipient, "552032534", "Old SAS", changes, CancellationToken.None);

        _handler.Requests.Should().HaveCount(1);
        using JsonDocument doc = JsonDocument.Parse(_handler.RequestBodies[0]);
        JsonElement root = doc.RootElement;
        root.GetProperty("subject").GetString().Should().Contain("Old SAS");

        string html = root.GetProperty("htmlContent").GetString()!;
        html.Should().Contain("Denomination");
        html.Should().Contain("Naf");
        html.Should().Contain("New SAS");

        string text = root.GetProperty("textContent").GetString()!;
        text.Should().Contain("552032534");
        text.Should().Contain("62.01Z");
    }

    [Fact]
    public async Task SendFeedRuleMatched_lists_matches_with_links()
    {
        BrevoEmailSender sender = CreateSender();
        var matches = new List<FeedRuleMatch>
        {
            new(FeedItemId.New(), "RGPD évolue", "https://example.test/a", "Résumé", "Le Monde", Now),
            new(FeedItemId.New(), "Une autre actu", null, null, "Les Échos", Now),
        };

        await sender.SendFeedRuleMatchedAsync(_recipient, "Mention RGPD", matches, CancellationToken.None);

        _handler.Requests.Should().HaveCount(1);
        using JsonDocument doc = JsonDocument.Parse(_handler.RequestBodies[0]);
        JsonElement root = doc.RootElement;
        root.GetProperty("subject").GetString().Should().Contain("Mention RGPD");
        root.GetProperty("subject").GetString().Should().Contain("2 nouveaux items");

        string html = root.GetProperty("htmlContent").GetString()!;
        html.Should().Contain("https://example.test/a");
        html.Should().Contain("Le Monde");

        // HtmlEncode échappe les non-ASCII en entité numérique (&#233; pour é) — on vérifie
        // donc le rendu humain via la version textContent qui reste en clair.
        string text = root.GetProperty("textContent").GetString()!;
        text.Should().Contain("RGPD évolue");
        text.Should().Contain("Une autre actu");
        text.Should().Contain("Les Échos");
    }

    [Fact]
    public async Task Send_does_not_throw_when_response_is_error()
    {
        BrevoEmailSender sender = CreateSender();
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"code\":\"bad\",\"message\":\"oops\"}"),
        };

        Func<Task> act = () => sender.SendEmailVerificationAsync(_recipient, UserId.New(), "t", CancellationToken.None);

        await act.Should().NotThrowAsync("un échec d'envoi email ne doit jamais casser le worker métier");
    }

    [Fact]
    public async Task Send_does_not_throw_when_handler_throws()
    {
        BrevoEmailSender sender = CreateSender();
        _handler.Responder = _ => throw new HttpRequestException("network down");

        Func<Task> act = () => sender.SendEmailVerificationAsync(_recipient, UserId.New(), "t", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Send_skips_when_sender_email_not_configured()
    {
        BrevoEmailSender sender = CreateSender(new BrevoOptions { ApiKey = "xkeysib-test", SenderEmail = null });

        await sender.SendEmailVerificationAsync(_recipient, UserId.New(), "t", CancellationToken.None);

        _handler.Requests.Should().BeEmpty("SenderEmail manquant → on s'arrête avant l'appel HTTP");
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> RequestBodies { get; } = [];

        public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.Created);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            RequestBodies.Add(body);
            Requests.Add(request);
            return Responder(request);
        }
    }
}
