using Microsoft.Extensions.DependencyInjection;

namespace TelegramBotFramework {

    /// <summary>
    /// Setup interface for Telegram Bots
    /// </summary>
    public interface IBotStartup {

        /// <summary>
        /// Configure dependency injection services
        /// </summary>
        /// <param name="services">Service collection</param>
        void ConfigureServices(IServiceCollection services);

    }
}
