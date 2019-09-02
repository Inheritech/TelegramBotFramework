using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotFramework.Attributes;
using TelegramBotFramework.Controllers;
using TelegramBotFramework.Runtime;
using TelegramBotFramework.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TelegramBotFramework.Handling {

    /// <summary>
    /// Text command handler for the bot
    /// </summary>
    public class CommandHandler {

        /// <summary>
        /// DI container used for resolving controllers
        /// </summary>
        private readonly IServiceProvider _diContainer;

        /// <summary>
        /// Registered controllers with associated command names
        /// </summary>
        private readonly Dictionary<string, ControllerMeta> _registeredControllers;

        /// <summary>
        /// Help command handler
        /// </summary>
        private readonly HelpCommand _helpCommand;

        /// <summary>
        /// Logger for this instance
        /// </summary>
        private readonly ILogger<CommandHandler> _logger;

        /// <summary>
        /// Initialize command handler with DI container
        /// </summary>
        /// <param name="container">DI container</param>
        public CommandHandler(IServiceProvider container, ILogger<CommandHandler> logger) {
            _logger = logger;
            _diContainer = container;
            _registeredControllers = new Dictionary<string, ControllerMeta>();
            _registeredControllers = ScanControllersInAsm();
            _helpCommand = new HelpCommand(_registeredControllers);
        }

        /// <summary>
        /// Handle bot message
        /// </summary>
        /// <param name="msg">Received message</param>
        public async Task HandleMessage(Message msg) {
            _logger.LogDebug($"Handling received message");
            var request = CommandRequest.Parse(msg);
            _logger.LogDebug("Handled request; Username: {Username}, Command: {Cmd}, Raw Parameters: {Params}", msg.From.Username, request.Command, request.RawParameters);
            await CallMostSuitableCommand(request);
        }

        /// <summary>
        /// Call the most suitable command
        /// </summary>
        /// <param name="request">Request made</param>
        private async Task CallMostSuitableCommand(CommandRequest request) {

            if (request.Command == "help") {
                _logger.LogInformation("Handling message as help command");
                await SendHelp(request);
                _logger.LogInformation("Sent help information to chat {ChatId}", request.Message.Chat.Id);
                return;
            }

            if (_registeredControllers.ContainsKey(request.Command)) {
                _logger.LogInformation("Found controller that matches request command");
                var ctrlMeta = _registeredControllers[request.Command];
                var mostSuitableVariant = ctrlMeta.GetMostSuitableVariant(request.Parameters);
                ArrayList invocationParams = null;

                if (mostSuitableVariant == null) {
                    _logger.LogInformation("No command variant could handle the required command");
                    return;
                }

                // Call best method
                if (mostSuitableVariant.ParameterCount == request.Parameters.Length) {
                    invocationParams = mostSuitableVariant.MatchParameters(request.Parameters);
                }
                // If the parameter count was 1 it would have been already called
                // then if it was not called yet it is 1 parameter, then, the default single parameter
                // variant was sent and we should use the full parameter string
                else if (mostSuitableVariant.ParameterCount == 1) {
                    invocationParams = new ArrayList {
                        request.RawParameters
                    };
                }

                // If the invocation params are null at this point, then we should try to call the method
                // parameterless, if the method is null, then the command string passed is invalid

                using var scope = _diContainer.CreateScope();

                // Build controller
                var controller = InitController(scope.ServiceProvider, ctrlMeta.ControllerType, request);


                _logger.LogInformation("Handling command with controller '{ControllerName}' on variant '{Method}'", ctrlMeta.ControllerType.FullName, mostSuitableVariant.Info.Name);
                await (dynamic)mostSuitableVariant.Info.Invoke(controller, invocationParams?.ToArray());
                _logger.LogInformation("Sent response to chat with ID {ChatId}", request.Message.Chat.Id);
            }
        }

        /// <summary>
        /// Run help command handler
        /// </summary>
        private async Task SendHelp(CommandRequest request) {
            await _helpCommand.SendHelpText(request, _diContainer.GetService<TelegramBotClient>());
        }

        /// <summary>
        /// Instantiate and init controller of a specific type
        /// </summary>
        /// <param name="t">Type to instantiate</param>
        /// <param name="request">Command request to set in the controller</param>
        private Controller InitController(IServiceProvider provider, Type t, CommandRequest request) {
            Controller instance = (Controller)ActivatorUtilities.CreateInstance(provider, t);
            instance.Initialize(_diContainer.GetService<TelegramBotClient>(), request);
            return instance;
        }

        /// <summary>
        /// Scan an assembly for Telegram bot controllers
        /// </summary>
        /// <param name="asm">Assembly to scan</param>
        private Dictionary<string, ControllerMeta> ScanControllersInAsm(Assembly asm = null) {
            if (asm == null)
                asm = Assembly.GetEntryAssembly();

            var possibleTypes = asm.GetTypes().Where(
                t => t.IsClass
                && !t.IsAbstract
                && t.IsSubclassOf(typeof(Controller))
            ).ToList();

            var result = new Dictionary<string, ControllerMeta>();

            foreach (Type type in possibleTypes) {
                var attr = type.GetCustomAttribute<BotCommandControllerAttribute>();
                if (attr != null) {
                    result.Add(attr.Name, new ControllerMeta(type));
                }
            }

            return result;
        }

    }
}
