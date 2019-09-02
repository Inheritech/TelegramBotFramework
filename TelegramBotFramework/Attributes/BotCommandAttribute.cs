using System;

namespace TelegramBotFramework.Attributes {

    /// <summary>
    /// Attribute for marking methods that should handle bot command variants
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BotCommandAttribute : Attribute {

        /// <summary>
        /// Usage description for this command variant
        /// </summary>
        public string Usage { get; }

        /// <summary>
        /// Mark this method as a command variant for a command controller
        /// </summary>
        /// <param name="usage">Usage description to show in help</param>
        public BotCommandAttribute(string usage) {
            Usage = usage;
        }

    }
}
