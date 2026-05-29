using System.Net;
using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Messaging.Email;
using Atlas.Infrastructure.Messaging.Push;
using Atlas.Infrastructure.Messaging.Push.Apns;
using Atlas.Infrastructure.Messaging.Push.Fcm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        // ── Push (F-020) ──────────────────────────────────────────────────────────────
        // Chaque plateforme configurée enregistre son IPlatformPushDispatcher ;
        // CompositeNotificationDispatcher fan-out vers tous. En l'absence de
        // plateforme configurée : fallback LoggingNotificationDispatcher (dev).

        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SectionName));
        services.Configure<ApnsOptions>(configuration.GetSection(ApnsOptions.SectionName));

        FcmOptions fcm = configuration.GetSection(FcmOptions.SectionName).Get<FcmOptions>() ?? new FcmOptions();
        ApnsOptions apns = configuration.GetSection(ApnsOptions.SectionName).Get<ApnsOptions>() ?? new ApnsOptions();

        bool fcmEnabled = !string.IsNullOrWhiteSpace(fcm.ProjectId)
            && !string.IsNullOrWhiteSpace(fcm.ServiceAccountJson);
        bool apnsEnabled = !string.IsNullOrWhiteSpace(apns.TeamId)
            && !string.IsNullOrWhiteSpace(apns.KeyId)
            && !string.IsNullOrWhiteSpace(apns.PrivateKeyPem)
            && !string.IsNullOrWhiteSpace(apns.BundleId);

        if (fcmEnabled)
        {
            services.AddHttpClient<IFcmAccessTokenProvider, FcmAccessTokenProvider>((_, client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(fcm.TimeoutSeconds);
            });

            services.AddHttpClient<IPlatformPushDispatcher, FcmNotificationDispatcher>((provider, client) =>
            {
                FcmOptions runtime = provider.GetRequiredService<IOptions<FcmOptions>>().Value;
                client.BaseAddress = new Uri("https://fcm.googleapis.com");
                client.Timeout = TimeSpan.FromSeconds(runtime.TimeoutSeconds);
            });
        }

        if (apnsEnabled)
        {
            services.AddSingleton<IApnsAccessTokenProvider, ApnsAccessTokenProvider>();

            services.AddHttpClient<IPlatformPushDispatcher, ApnsNotificationDispatcher>((provider, client) =>
            {
                ApnsOptions runtime = provider.GetRequiredService<IOptions<ApnsOptions>>().Value;
                client.BaseAddress = new Uri(runtime.UseSandbox
                    ? "https://api.sandbox.push.apple.com"
                    : "https://api.push.apple.com");
                client.Timeout = TimeSpan.FromSeconds(runtime.TimeoutSeconds);
                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            });
        }

        if (fcmEnabled || apnsEnabled)
        {
            services.AddScoped<INotificationDispatcher, CompositeNotificationDispatcher>();
        }
        else
        {
            services.AddScoped<INotificationDispatcher, LoggingNotificationDispatcher>();
        }

        return services;
    }
}
