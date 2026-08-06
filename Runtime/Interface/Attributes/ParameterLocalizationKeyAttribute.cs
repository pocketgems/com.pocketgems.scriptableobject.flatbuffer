using System;

namespace PocketGems.Parameters.Interface.Attributes
{
    /// <summary>
    /// Used on string properties or string collections to tag them as runtime LocalizedString properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterLocalizationKeyAttribute : Attribute { }
}
