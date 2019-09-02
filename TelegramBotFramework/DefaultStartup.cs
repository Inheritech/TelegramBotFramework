using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace TelegramBotFramework {

    /// <summary>
    /// Default configuration startup
    /// </summary>
    public class DefaultStartup : IBotStartup {

        /// <summary>
        /// Configure dependency injection services
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        public void ConfigureServices(IServiceCollection services) {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = configurationBuilder.Build();

            services.AddLogging(builder => {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });
        }
    }
}
