using System;

namespace PocketGems.Parameters.Interface
{
    /// <summary>
    /// Attribute to be attached to properties on parameter interfaces.
    ///
    /// The attribute text are appended to the Scriptable Object fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AttachFieldAttributeAttribute : Attribute
    {
        public string AttributeText;
        public AttachFieldAttributeAttribute(string attributeText)
        {
            AttributeText = attributeText;
        }
    }
}
