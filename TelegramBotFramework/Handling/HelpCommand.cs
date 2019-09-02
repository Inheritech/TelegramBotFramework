using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBotFramework.Attributes;
using TelegramBotFramework.Runtime;
using TelegramBotFramework.Types;

namespace TelegramBotFramework.Handling {

    /// <summary>
    /// Command handler specific for the help command on the bot
    /// </summary>
    public class HelpCommand {

        /// <summary>
        /// Cached non verbose help text
        /// </summary>
        private readonly string _nonVerboseCache;

        /// <summary>
        /// Cached verbose help text
        /// </summary>
        private readonly string _verboseCache;

        /// <summary>
        /// Initialize help text command
        /// </summary>
        /// <param name="controllers">Registered controllers for which to generate help information</param>
        public HelpCommand(Dictionary<string, ControllerMeta> controllers) {
            _verboseCache = GenerateHelpText(controllers, true);
            _nonVerboseCache = GenerateHelpText(controllers);
        }

        /// <summary>
        /// Send help text based on current request
        /// </summary>
        /// <param name="request">Received request</param>
        /// <param name="client">Telegram bot client</param>
        /// <param name="controllers">Registered controllers</param>
        public async Task SendHelpText(CommandRequest request, TelegramBotClient client) {
            string message;
            if (request.RawParameters == "verbose") {
                message = _verboseCache;
            } else {
                message = _nonVerboseCache;
            }
            await client.SendTextMessageAsync(request.Message.Chat.Id, message, Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        /// <summary>
        /// Generate the full help text
        /// </summary>
        /// <param name="registeredControllers">Commands for which to generate help information</param>
        /// <param name="verbose">If verbose, this will add possible values to the help text</param>
        private string GenerateHelpText(Dictionary<string, ControllerMeta> registeredControllers, bool verbose = false) {
            StringBuilder builder = new StringBuilder();
            var execAsm = Assembly.GetEntryAssembly();
            var execAsmName = execAsm.GetName();
            builder.AppendLine($"{execAsmName.Name} {execAsmName.Version} Help Text");
            builder.AppendLine();
            builder.AppendLine("Available commands: ");

            foreach (ControllerMeta meta in registeredControllers.Values) {
                builder.AppendLine($"/{meta.CommandName}");
                builder.AppendLine($"  Description: {meta.CommandDescription}");
                builder.AppendLine("_Command Variants:_ ");

                foreach (ControllerMethodMeta methodMeta in meta.Methods) {
                    builder.AppendLine($"  *Name*: {methodMeta.Info.Name}");
                    builder.AppendLine($"  *Usage*: {methodMeta.CommandVariantUsage}");
                    builder.AppendLine($"  *Parameters*:");

                    foreach (ParameterInfo info in methodMeta.Parameters) {
                        builder.AppendLine($"    `{info.Name.ToUpperInvariant()}` - {GetParamHelpText(info)}");
                        // Always show possible enum values
                        if (info.ParameterType.IsEnum || verbose) {
                            string[] possibleValues = GetParamTypeValues(info.ParameterType);
                            foreach (string possibleValue in possibleValues) {
                                builder.AppendLine($"      > {possibleValue}");
                            }
                        }
                    }
                    builder.AppendLine();
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get a human friendly help text for a param
        /// </summary>
        /// <param name="info">Parameter for which to get the help text</param>
        private string GetParamHelpText(ParameterInfo info) {

            var helpTextAttr = info.GetCustomAttribute<ParameterHelpAttribute>();
            if (helpTextAttr != null) {
                return helpTextAttr.HelpText;
            }

            if (info.ParameterType.IsEnum) {
                return info.ParameterType.Name;
            }

            switch (Type.GetTypeCode(info.ParameterType)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return "Integer";
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return "Decimal";
                case TypeCode.Boolean:
                    return "Boolean";
                case TypeCode.Char:
                    return "Char";
                case TypeCode.DateTime:
                    return "DateTime";
            }

            return "Text";
        }

        /// <summary>
        /// If the type has specific possible values, list them
        /// in here
        /// </summary>
        /// <param name="t">Checked type</param>
        private string[] GetParamTypeValues(Type t) {

            if (t.IsEnum) {
                return Enum.GetNames(t);
            }

            switch (Type.GetTypeCode(t)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return new string[] {
                        "Any integer number",
                        "0",
                        "1",
                        "42"
                    };
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return new string[] {
                        "Any decimal number",
                        "0.5",
                        "1.12",
                        "42.5"
                    };
                case TypeCode.Boolean:
                    return new string[] {
                        "true",
                        "false",
                        "1",
                        "0"
                    };
                case TypeCode.Char:
                    return new string[] {
                        "Any single character",
                        "a",
                        "z",
                        "b"
                    };
                case TypeCode.DateTime:
                    return new string[] {
                        "A correctly formatted date, time or both"
                    };
            }

            return new string[] {
                "Any text"
            };
        }

    }
}
