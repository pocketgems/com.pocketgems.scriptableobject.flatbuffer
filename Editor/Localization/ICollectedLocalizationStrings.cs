using System.Collections.Generic;

namespace PocketGems.Parameters.Editor.Localization
{
    public interface ICollectedLocalizationStrings
    {
        /// <summary>
        /// All unique localization keys values of parameter interface properties that are tagged with the
        /// [ParameterLocalizationKey] attribute;
        /// </summary>
        IEnumerable<string> LocalizationKeys { get; }

        /// <summary>
        /// All unique localization script values of parameter interface properties that are tagged with the
        /// [ParameterLocalizableScriptAttribute] attribute;
        /// </summary>
        IEnumerable<string> LocalizableScripts { get; }
    }
}
