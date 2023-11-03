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
        public delegate string TranslateStringDelegate(string localizationKey);
        public static TranslateStringDelegate GlobalTranslateStringDelegate;

        /// <summary>
        /// Called by the auto generated parameters info files to translate strings.
        /// </summary>
        /// <param name="localizationKey">The localization key to fetch the local language's text for.</param>
        /// <returns>The current locale's translation if it exists, else the key is returned.</returns>
        public static string GetTranslation(string localizationKey)
        {
            if (string.IsNullOrWhiteSpace(localizationKey))
            {
                return localizationKey;
            }

            if (GlobalTranslateStringDelegate == null)
            {
                Debug.LogError($"{nameof(GlobalTranslateStringDelegate)} not set prior to calling {nameof(GetTranslation)}");
                return localizationKey;
            }

            return GlobalTranslateStringDelegate(localizationKey.Trim());
        }
    }
}
