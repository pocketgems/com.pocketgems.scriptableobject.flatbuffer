using System.Collections.Generic;

namespace PocketGems.Parameters.Editor.Localization
{
    public interface ICollectedLocalizationStrings
    {
        enum State
        {
            Success,
            Error
        }

        /// <summary>
        /// The state of the results of attempting to collect strings.
        /// </summary>
        State Result { get; }

        /// <summary>
        /// The error message if the string collection resulted in an Error.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// If successful, all unique localization keys values of parameter interface properties that are tagged with the
        /// [ParameterLocalizationKey] attribute;
        ///
        /// Otherwise, null on error.
        /// </summary>
        IEnumerable<string> LocalizationKeys { get; }

        /// <summary>
        /// If successful, all unique localization script values of parameter interface properties that are tagged with the
        /// [ParameterLocalizableScriptAttribute] attribute;
        ///
        /// Otherwise, null on error.
        /// </summary>
        IEnumerable<string> LocalizableScripts { get; }
    }
}
