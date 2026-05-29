using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Messaging.Email;
using Atlas.Infrastructure.Messaging.Push;
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

        // Push (F-020) : si FCM est configuré, on enregistre l'adapter FCM (Android + Web).
        // Sinon, fallback sur LoggingNotificationDispatcher (dev / hors prod).
        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SectionName));
        FcmOptions fcm = configuration.GetSection(FcmOptions.SectionName).Get<FcmOptions>() ?? new FcmOptions();
        bool fcmEnabled = !string.IsNullOrWhiteSpace(fcm.ProjectId)
            && !string.IsNullOrWhiteSpace(fcm.ServiceAccountJson);

        if (fcmEnabled)
        {
            services.AddHttpClient<IFcmAccessTokenProvider, FcmAccessTokenProvider>((_, client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(fcm.TimeoutSeconds);
            });

            services.AddHttpClient<INotificationDispatcher, FcmNotificationDispatcher>((provider, client) =>
            {
                FcmOptions runtime = provider.GetRequiredService<IOptions<FcmOptions>>().Value;
                client.BaseAddress = new Uri("https://fcm.googleapis.com");
                client.Timeout = TimeSpan.FromSeconds(runtime.TimeoutSeconds);
            });
        }
        else
        {
            services.AddScoped<INotificationDispatcher, LoggingNotificationDispatcher>();
        }

        return services;
    }
}
