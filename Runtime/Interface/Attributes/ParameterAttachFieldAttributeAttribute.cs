using System;
using PocketGems.Parameters.Interface.Attributes;

namespace PocketGems.Parameters.Interface.Attributes
{
    /// <summary>
    /// Attribute to be attached to properties on parameter interfaces.
    ///
    /// The attribute text are appended to the Scriptable Object fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ParameterAttachFieldAttributeAttribute : Attribute, IAttachScriptableObjectAttribute
    {
        private readonly string _attributeText;

        public ParameterAttachFieldAttributeAttribute(string attributeText)
        {
            _attributeText = attributeText;
        }

        string IAttachScriptableObjectAttribute.ScriptableObjectFieldAttributesCode => _attributeText;
    }
}
