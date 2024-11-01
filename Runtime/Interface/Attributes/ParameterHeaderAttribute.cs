using System;

namespace PocketGems.Parameters.Interface.Attributes
{
    /// <summary>
    /// Attribute to generate a Header on the Scriptable Object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ParameterHeaderAttribute : System.Attribute, IAttachScriptableObjectAttribute
    {
        private readonly string _headerText;

        /// <summary>
        /// Attribute to generate a Header on the Scriptable Object.
        /// </summary>
        /// <param name="header">The text generated on the Scriptable Object's header attribute.</param>
        public ParameterHeaderAttribute(string header)
        {
            _headerText = header;
        }

        string IAttachScriptableObjectAttribute.ScriptableObjectFieldAttributesCode => $"[Header(\"{_headerText}\")]";
    }
}
