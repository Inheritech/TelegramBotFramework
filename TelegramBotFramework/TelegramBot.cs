using Microsoft.Extensions.Logging;
using System;
using Telegram.Bot;
using TelegramBotFramework.Handling;

namespace TelegramBotFramework {

    /// <summary>
    /// Class for creating new Telegram Bots
    /// </summary>
    public class TelegramBot {

        /// <summary>
        /// API client used for this bot instance
        /// </summary>
        public TelegramBotClient Client { get; }

        /// <summary>
        /// Services used by this bot
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Command handler
        /// </summary>
        private readonly CommandHandler _commandHandler;

        /// <summary>
        /// Logger for this instance
        /// </summary>
        private readonly ILogger<TelegramBot> _logger;

        /// <summary>
        /// Create a new bot instance
        /// </summary>
        /// <param name="apiToken">Token to use for this bot</param>
        public TelegramBot(TelegramBotClient client, CommandHandler handler, IServiceProvider services, ILogger<TelegramBot> logger) {
            Client = client;
            _commandHandler = handler;
            _logger = logger;
            Services = services;
            client.OnMessage += Client_OnMessage;
        }

        /// <summary>
        /// Handle message sent to the bot
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Message arguments</param>
        private async void Client_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e) {
            if (e.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text) return; // TODO: Implement multiple types of handlers

            _logger.LogInformation("Received message, using CommandHandler to handle this message");
            await _commandHandler.HandleMessage(e.Message);
        }

        /// <summary>
        /// <para>Start the bot with the default parameters</para>
        /// <remarks>You can call <see cref="Client"/>'s Start method directly to set
        /// additional parameters</remarks>
        /// </summary>
        public TelegramBot Start() {
            Client.StartReceiving();
            _logger.LogInformation("Initialized bot");
            return this;
        }
    }
}
