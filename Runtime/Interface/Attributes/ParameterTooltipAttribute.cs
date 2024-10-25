using System;

namespace PocketGems.Parameters.Interface.Attributes
{
    /// <summary>
    /// Attribute to generate a Tooltip on the Scriptable Object.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ParameterTooltipAttribute : System.Attribute, IAttachScriptableObjectAttribute
    {
        private readonly string _tooltipText;

        /// <summary>
        /// Attribute to generate a Tooltip on the Scriptable Object.
        /// </summary>
        /// <param name="header">The text generated on the Scriptable Object's tooltip attribute.</param>
        public ParameterTooltipAttribute(string tooltip)
        {
            _tooltipText = tooltip;
        }

        string IAttachScriptableObjectAttribute.ScriptableObjectFieldAttributesCode => $"[Tooltip(\"{_tooltipText}\")]";
    }
}
