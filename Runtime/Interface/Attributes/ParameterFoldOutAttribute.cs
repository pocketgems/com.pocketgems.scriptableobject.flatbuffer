using System;

namespace PocketGems.Parameters.Interface.Attributes
{
    /// <summary>
    /// Attribute to generate an editor foldout around all properties below this one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ParameterFoldOutAttribute : System.Attribute, IAttachScriptableObjectAttribute
    {
        public readonly string NameText;
        public readonly bool InitialFoldout;

        /// <summary>
        /// Attribute to generate an editor foldout around all properties below this one.
        /// </summary>
        /// <param name="name">Name displayed above the fold out</param>
        /// <param name="initialFoldout">true if the initial foldout state is open, false otherwise</param>
        public ParameterFoldOutAttribute(string name, bool initialFoldout = true)
        {
            NameText = name;
            InitialFoldout = initialFoldout;
        }

        string IAttachScriptableObjectAttribute.ScriptableObjectFieldAttributesCode => $"[{nameof(ParameterFoldOutAttribute)}(\"{NameText}\", {InitialFoldout.ToString().ToLower()})]";
    }
}
