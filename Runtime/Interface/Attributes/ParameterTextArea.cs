using System;

namespace PocketGems.Parameters.Interface.Attributes
{
    /// <summary>
    /// Attribute to generate a TextArea on the Scriptable Object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterTextAreaAttribute : System.Attribute, IAttachScriptableObjectAttribute
    {
        private readonly bool _hasArgs;
        private readonly int _minLines;
        private readonly int _maxLines;

        public ParameterTextAreaAttribute()
        {
            _hasArgs = false;
        }

        public ParameterTextAreaAttribute(int minLines, int maxLines)
        {
            _hasArgs = true;
            _minLines = minLines;
            _maxLines = maxLines;
        }

        string IAttachScriptableObjectAttribute.ScriptableObjectFieldAttributesCode
        {
            get
            {
                if (_hasArgs)
                    return $"[TextArea({_minLines}, {_maxLines})]";
                return "[TextArea]";
            }
        }
    }
}
