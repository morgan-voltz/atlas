using Atlas.Domain.Notifications;
using Atlas.Infrastructure.Messaging.Email;
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

        return services;
    }
}
