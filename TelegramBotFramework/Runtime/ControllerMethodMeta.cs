using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using TelegramBotFramework.Attributes;

namespace TelegramBotFramework.Runtime {

    /// <summary>
    /// Metadata for controller methods
    /// </summary>
    public class ControllerMethodMeta {

        /// <summary>
        /// Method information
        /// </summary>
        public MethodInfo Info { get; }

        /// <summary>
        /// Usage for this command variant
        /// </summary>
        public string CommandVariantUsage { get; }

        /// <summary>
        /// <para>Total parameters for this method</para>
        /// </summary>
        public int ParameterCount { get; }

        /// <summary>
        /// Total required parameters for this method
        /// </summary>
        public int RequiredParameterCount { get; }

        /// <summary>
        /// Method parameters
        /// </summary>
        public ParameterInfo[] Parameters { get; }

        /// <summary>
        /// Initialize metadata
        /// </summary>
        /// 
        public ControllerMethodMeta(MethodInfo info) {
            Info = info;
            Parameters = info.GetParameters();
            ParameterCount = Parameters.Length;
            RequiredParameterCount = Parameters.Count(p => !p.IsOptional);
            var attr = info.GetCustomAttribute<BotCommandAttribute>();
            CommandVariantUsage = attr.Usage;
        }

        /// <summary>
        /// Try to match string parameters with this method's parameter signature
        /// </summary>
        /// <param name="parameters">Parameters to use</param>
        /// <returns>Result converted parameters to use or null if parameters cannot be matched</returns>
        public ArrayList MatchParameters(string[] parameters) {
            if (parameters.Length == 0)
                return new ArrayList();

            try {
                ArrayList list = new ArrayList();
                for(int i = 0; i < parameters.Length; i++) {
                    var paramType = Parameters[i].ParameterType;
                    if (paramType.IsEnum && Enum.TryParse(paramType, parameters[i], out object enumResult)) {
                        if (enumResult != null) {
                            list.Add(enumResult);
                        }
                    } else {
                        list.Add(Convert.ChangeType(parameters[i], Parameters[i].ParameterType));
                    }
                }
                return list;
            } catch(Exception) {
                return new ArrayList();
            }
        }

    }
}
