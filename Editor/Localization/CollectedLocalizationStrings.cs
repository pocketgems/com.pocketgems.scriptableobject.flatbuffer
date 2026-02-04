using System.Collections.Generic;

namespace PocketGems.Parameters.Editor.Localization
{
    internal class CollectedLocalizationStrings : ICollectedLocalizationStrings
    {
        public ICollectedLocalizationStrings.State Result { get; }
        public string ErrorMessage { get; }
        public IEnumerable<string> LocalizationKeys { get; }
        public IEnumerable<string> LocalizableScripts { get; }

        public CollectedLocalizationStrings(string errorMessage)
        {
            Result = ICollectedLocalizationStrings.State.Error;
            ErrorMessage = errorMessage;
        }

        public CollectedLocalizationStrings(HashSet<string> localizationKeys, HashSet<string> localizableScripts)
        {
            Result = ICollectedLocalizationStrings.State.Success;
            LocalizationKeys = localizationKeys;
            LocalizableScripts = localizableScripts;
        }
    }
}
