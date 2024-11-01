using System;
using PocketGems.Parameters.Interface.Attributes;

namespace PocketGems.Parameters.Interface
{
    /// <summary>
    /// Attribute to be attached to properties on parameter interfaces.
    ///
    /// The attribute text are appended to the Scriptable Object fields.
    /// </summary>
    [Obsolete("Use ParameterAttachFieldAttributeAttribute instead")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AttachFieldAttributeAttribute : Attribute, IAttachScriptableObjectAttribute
    {
        public string AttributeText;

        public AttachFieldAttributeAttribute(string attributeText)
        {
            AttributeText = attributeText;
        }

        string IAttachScriptableObjectAttribute.ScriptableObjectFieldAttributesCode => AttributeText;
    }
}
