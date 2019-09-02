using Telegram.Bot;
using TelegramBotFramework.Types;

namespace TelegramBotFramework.Controllers {

    /// <summary>
    /// Base class for controllers that handle Telegram messages
    /// </summary>
    public class Controller {

        /// <summary>
        /// Current bot handling this request
        /// </summary>
        public TelegramBotClient Bot { get; private set; }

        /// <summary>
        /// Current handled request if available
        /// </summary>
        public CommandRequest Request { get; private set; }

        /// <summary>
        /// Initialize controller with request data and bot client
        /// </summary>
        /// <param name="bot">Bot for this controller</param>
        /// <param name="request">Message handled</param>
        internal void Initialize(TelegramBotClient bot, CommandRequest request) {
            Bot = bot;
            Request = request;
        }
    }
}
