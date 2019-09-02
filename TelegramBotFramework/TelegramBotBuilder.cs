using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramBotFramework.Handling;

namespace TelegramBotFramework {
    /// <summary>
    /// Builder class for telegram bots
    /// </summary>
    public class TelegramBotBuilder {

        /// <summary>
        /// Services to use for the bot
        /// </summary>
        private readonly IServiceCollection _services;

        /// <summary>
        /// Initialize builder
        /// </summary>
        /// <param name="apiToken">Token to use for this bot</param>
        public TelegramBotBuilder(string apiToken) {
            var bot = new TelegramBotClient(apiToken);

            _services = new ServiceCollection()
                .AddSingleton<CommandHandler>()
                .AddSingleton(bot);

            UseStartup<DefaultStartup>();
        }

        /// <summary>
        /// Use bot startup class
        /// </summary>
        /// <typeparam name="T">Type of the class to use</typeparam>
        public TelegramBotBuilder UseStartup<T>()
            where T : IBotStartup, new() 
        {
            IBotStartup startup = new T();
            startup.ConfigureServices(_services);
            return this;
        }

        /// <summary>
        /// Build telegram bot
        /// </summary>
        public TelegramBot Build() {
            var provider = _services.BuildServiceProvider();
            return ActivatorUtilities.CreateInstance<TelegramBot>(provider);
        }

    }
}
