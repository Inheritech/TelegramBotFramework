using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TelegramBotFramework.Attributes;

namespace TelegramBotFramework.Runtime {

    /// <summary>
    /// Class for handling the metadata of a controller
    /// </summary>
    public class ControllerMeta {

        /// <summary>
        /// Command name for this controller
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Command description for this controller
        /// </summary>
        public string CommandDescription { get; }

        /// <summary>
        /// Instance of the controller
        /// </summary>
        public Type ControllerType { get; }

        /// <summary>
        /// Method to be invoked when no parameters are sent
        /// </summary>
        public ControllerMethodMeta DefaultParameterless { get; }

        /// <summary>
        /// Method to be invoked when a single parameter is sent
        /// or no other method can handle the parameters
        /// </summary>
        public ControllerMethodMeta DefaultSingleParameter { get; }

        /// <summary>
        /// Controller command methods metadata
        /// </summary>
        public ControllerMethodMeta[] Methods { get; }

        /// <summary>
        /// Initialize managed controller with a specific controller instance
        /// </summary>
        /// <param name="controllerType">Controller type to use</param>
        public ControllerMeta(Type controllerType) {
            ControllerType = controllerType;
            var attr = controllerType.GetCustomAttribute<BotCommandControllerAttribute>();

            CommandName = attr.Name;
            CommandDescription = attr.Description;

            Methods = GetAvailableMethods(controllerType);
            DefaultParameterless = GetDefaultParameterless();
            DefaultSingleParameter = GetDefaultWithSingleParameter();
        }

        /// <summary>
        /// Gets the most suitable variant of this command based on the string parameter list
        /// </summary>
        /// <param name="parameters">Parameters to use for invoking the command variant method</param>
        /// <returns>The most suitable method or null if no method was suitable</returns>
        public ControllerMethodMeta GetMostSuitableVariant(string[] parameters) {
            var availableCommandVariants = Methods.Where(cmm => cmm.ParameterCount == parameters.Length);
            if (availableCommandVariants.Any()) {
                foreach (var commandVariant in availableCommandVariants) {
                    var convertedParams = commandVariant.MatchParameters(parameters);
                    if (convertedParams != null && commandVariant != DefaultSingleParameter && commandVariant != DefaultParameterless)
                        return commandVariant;
                }
            }
            if (DefaultSingleParameter != null) {
                return DefaultSingleParameter;
            }
            if (DefaultParameterless != null) {
                return DefaultParameterless;
            }
            return null;
        }

        /// <summary>
        /// Get all available command methods in the type
        /// </summary>
        /// <param name="t">Type to search</param>
        private ControllerMethodMeta[] GetAvailableMethods(Type t) {
            List<ControllerMethodMeta> result = new List<ControllerMethodMeta>();
            var methods = t.GetMethods();
            foreach (MethodInfo info in methods) {
                if (info.GetCustomAttribute<BotCommandAttribute>() != null) {
                    result.Add(new ControllerMethodMeta(info));
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Get the default method to invoke when this controller command
        /// is called parameterless
        /// </summary>
        private ControllerMethodMeta GetDefaultParameterless() {
            return Methods.FirstOrDefault(cmm => cmm.ParameterCount == 0);
        }

        /// <summary>
        /// Get the default method to invoke when this controller command
        /// is called with a single string parameter or the command is called
        /// with an arbitrary list of parameters that did not match any other
        /// method
        /// </summary>
        private ControllerMethodMeta GetDefaultWithSingleParameter() {
            var possibleMethods = Methods.Where(cmm => cmm.ParameterCount == 1);
            foreach (ControllerMethodMeta info in possibleMethods) {
                var param = info.Parameters[0];
                if (param.ParameterType == typeof(string)) {
                    return info;
                }
            }
            return null;
        }
    }
}
