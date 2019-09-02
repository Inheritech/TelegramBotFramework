using System;

namespace TelegramBotFramework.Attributes {
    /// <summary>
    /// Attribute for defining custom information for each parameter
    /// shown in the help text
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterHelpAttribute : Attribute {

        /// <summary>
        /// Help text to show for this parameter
        /// </summary>
        public string HelpText { get; }

        public ParameterHelpAttribute(string helpText) {
            HelpText = helpText;
        }

    }
}
