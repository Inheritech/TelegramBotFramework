using System;

namespace TelegramBotFramework.Attributes {

    /// <summary>
    /// Attribute for marking controllers that should handle commands
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BotCommandControllerAttribute : Attribute {

        /// <summary>
        /// Command name that the controller handles
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Command description to use in the help command
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Set the command name that this controller should handle
        /// </summary>
        /// <param name="name">Command name to handle</param>
        public BotCommandControllerAttribute(string name, string description = null) {
            Name = name;
            Description = description;
        }
    }
}
