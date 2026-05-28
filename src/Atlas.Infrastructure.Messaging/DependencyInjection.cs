using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Messaging.Email;
using Atlas.Infrastructure.Messaging.Push;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        // Dispatcher push par défaut (Lot F-020) : log les notifications. Remplacé par les adapters
        // FCM / APNs / WNS dans des PRs séparées par plateforme.
        services.AddScoped<INotificationDispatcher, LoggingNotificationDispatcher>();

        return services;
    }
}
