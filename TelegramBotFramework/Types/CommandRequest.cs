using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using TelegramBotFramework.Extensions;

namespace TelegramBotFramework.Types {

    /// <summary>
    /// Command request containing information about the
    /// message text and parsed information
    /// </summary>
    public class CommandRequest {

        /// <summary>
        /// Message received
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// Command name if exists
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Raw parameter string
        /// </summary>
        public string RawParameters { get; private set; }

        /// <summary>
        /// Received parameters
        /// </summary>
        public string[] Parameters { get; private set; }

        private CommandRequest(Message msg) {
            Message = msg;
        }

        /// <summary>
        /// Parse a Telegram Message into a <see cref="CommandRequest"/>
        /// </summary>
        /// <param name="msg">Message to parse</param>
        public static CommandRequest Parse(Message msg) {
            var result = new CommandRequest(msg);
            var text = msg.Text;

            if (text != null) {
                if (text.StartsWith('/')) {
                    text = text.Remove(0, 1);
                }

                // TODO: Move transformation logic to another method or file
                var command = text.Split(new char[0], StringSplitOptions.None)[0];
                var parameterStr = text.Remove(0, command.Length).Trim();

                // Separate parameters by space or quotes (Hello "Great World" => 2 parameters)
                var parameters = Regex.Matches(parameterStr, @"[\""].+?[\""]|[^ ]+")
                                    .Cast<Match>()
                                    .Select(m => m.Value)
                                    .ToArray();

                // Clean parameters (remove surrounding quotes in commands with spaces)
                parameters = parameters.Select(p => {
                    if (p.Contains(' ')) { // If the parameter contains spaces, it means, it is surrounded by quotes
                        p = p.Remove(0, 1);
                        p = p.Remove(p.Length - 1, 1);
                    }
                    p = p.Trim();
                    return p;
                }).ToArray();

                command = command.Trim();
                
                // Remove bot name from command
                if (command.Contains('@')) {
                    command = command.Split('@').Splice(-1).Join();
                }

                result.Command = command;
                result.Parameters = parameters;
                result.RawParameters = parameterStr;
            }

            return result;
        }

    }
}
