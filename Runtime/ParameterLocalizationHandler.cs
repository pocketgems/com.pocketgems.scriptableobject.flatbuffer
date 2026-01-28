using UnityEngine;

namespace PocketGems.Parameters
{
    /// <summary>
    /// Static handler that is used by auto generated code for localizing parameter LocalizedStrings &
    /// Array LocalizedString types.
    /// </summary>
    public static class ParameterLocalizationHandler
    {
        /// <summary>
        /// Callback that can be set externally by apps for a localization system to translate given a localization
        /// key.
        /// </summary>
        public delegate string TranslateLocalizationKeyDelegate(string localizationKey);
        public static TranslateLocalizationKeyDelegate GlobalTranslateLocalizationKeyDelegate;

        /// <summary>
        /// Callback that can be set externally by apps for a localization system to translate given a script.
        /// </summary>
        public delegate string TranslateLocalizableScriptDelegate(string localizableScript);
        public static TranslateLocalizableScriptDelegate GlobalTranslateLocalizableScriptDelegate;

        /// <summary>
        /// Called by the auto generated parameters info files to translate strings.
        /// </summary>
        /// <param name="localizationKey">The localization key to fetch the local language's text for.</param>
        /// <returns>The current locale's translation if it exists, else the key is returned.</returns>
        public static string GetLocalizationKeyTranslation(string localizationKey)
        {
            if (string.IsNullOrWhiteSpace(localizationKey))
                return localizationKey;

            if (GlobalTranslateLocalizationKeyDelegate == null)
            {
                Debug.LogError($"{nameof(GlobalTranslateLocalizationKeyDelegate)} not set prior to calling {nameof(GetLocalizationKeyTranslation)}");
                return localizationKey;
            }

            return GlobalTranslateLocalizationKeyDelegate(localizationKey);
        }

        /// <summary>
        /// Called by the auto generated parameters info files to translate strings.
        /// </summary>
        /// <param name="localizableScript">The localizableScript to fetch a localized script for.</param>
        /// <returns>The current locale's translation if it exists, else the key is returned.</returns>
        public static string GetLocalizableScriptTranslation(string localizableScript)
        {
            if (string.IsNullOrWhiteSpace(localizableScript))
                return localizableScript;

            if (GlobalTranslateLocalizableScriptDelegate == null)
            {
                Debug.LogError($"{nameof(GlobalTranslateLocalizableScriptDelegate)} not set prior to calling {nameof(GetLocalizableScriptTranslation)}");
                return localizableScript;
            }

            return GlobalTranslateLocalizableScriptDelegate(localizableScript);
        }
    }
}
