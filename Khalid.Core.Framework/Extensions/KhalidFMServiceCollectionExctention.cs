using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Khalid.Core.Framework
{
    public static class KhalidFMServiceCollectionExctention
    {
        public static IServiceCollection AddEmailService(
                [NotNull] this IServiceCollection services,
                [NotNull] IConfiguration configuration,
                string envirnomentName)
                    {
                        var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
                        services.AddSingleton(s => new EmailService(emailConfig));
                        return services;
                    }

        public static IServiceCollection AddMobileService(
        [NotNull] this IServiceCollection services,
        [NotNull] IConfiguration configuration,
        string envirnomentName)
        {
            var emailConfig = configuration.GetSection("MobileConfiguration").Get<MobileConfiguration>();
            services.AddSingleton(s => new MobileNotificationService(emailConfig));
            return services;
        }
    }
}
