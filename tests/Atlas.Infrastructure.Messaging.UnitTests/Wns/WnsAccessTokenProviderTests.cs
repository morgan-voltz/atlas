using System.Net;
using Atlas.Infrastructure.Messaging.Push.Wns;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging.UnitTests.Wns;

public sealed class WnsAccessTokenProviderTests : IDisposable
{
    private readonly FakeHttpMessageHandler _handler = new();

    public void Dispose() => _handler.Dispose();

    private WnsAccessTokenProvider CreateProvider(WnsOptions? options = null)
    {
        var httpClient = new HttpClient(_handler);
        return new WnsAccessTokenProvider(httpClient, Options.Create(options ?? NewOptions()));
    }

    private static WnsOptions NewOptions() => new()
    {
        PackageSid = "ms-app://s-1-15-2-1234567890-1234567890",
        ClientSecret = "SECRET-XXX",
        TimeoutSeconds = 30,
    };

    [Fact]
    public async Task GetAccessTokenAsync_posts_client_credentials_form_to_login_live_com()
    {
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"access_token\":\"WNS-TOK\",\"expires_in\":86400}",
                System.Text.Encoding.UTF8, "application/json"),
        };

        string token = await CreateProvider().GetAccessTokenAsync();

        token.Should().Be("WNS-TOK");
        _handler.Requests.Should().HaveCount(1);
        _handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        _handler.Requests[0].RequestUri!.AbsoluteUri.Should().Be("https://login.live.com/accesstoken.srf");

        string body = _handler.RequestBodies[0];
        body.Should().Contain("grant_type=client_credentials");
        body.Should().Contain("scope=notify.windows.com");
        // Le SID est URL-encoded (les ':' et '/' sont échappés).
        body.Should().Contain("client_id=");
        body.Should().Contain("client_secret=SECRET-XXX");
    }

    [Fact]
    public async Task GetAccessTokenAsync_caches_the_token_for_subsequent_calls()
    {
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"access_token\":\"WNS-CACHED\",\"expires_in\":86400}",
                System.Text.Encoding.UTF8, "application/json"),
        };

        WnsAccessTokenProvider provider = CreateProvider();

        string first = await provider.GetAccessTokenAsync();
        string second = await provider.GetAccessTokenAsync();

        first.Should().Be("WNS-CACHED");
        second.Should().Be("WNS-CACHED");
        _handler.Requests.Should().HaveCount(1); // un seul appel OAuth2, le 2ᵉ est servi du cache.
    }

    [Fact]
    public async Task GetAccessTokenAsync_throws_when_PackageSid_is_missing()
    {
        WnsAccessTokenProvider provider = CreateProvider(new WnsOptions
        {
            PackageSid = null,
            ClientSecret = "secret",
        });

        Func<Task> act = async () => await provider.GetAccessTokenAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PackageSid*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_throws_when_ClientSecret_is_missing()
    {
        WnsAccessTokenProvider provider = CreateProvider(new WnsOptions
        {
            PackageSid = "ms-app://s-1-15-2-1",
            ClientSecret = null,
        });

        Func<Task> act = async () => await provider.GetAccessTokenAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ClientSecret*");
    }

    [Fact]
    public async Task GetAccessTokenAsync_throws_when_response_has_no_access_token()
    {
        _handler.Responder = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"expires_in\":3600}", System.Text.Encoding.UTF8, "application/json"),
        };

        Func<Task> act = async () => await CreateProvider().GetAccessTokenAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*access_token*");
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> RequestBodies { get; } = [];

        public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
