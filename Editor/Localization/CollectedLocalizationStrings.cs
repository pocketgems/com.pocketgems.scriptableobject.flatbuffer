using System.Collections.Generic;

namespace PocketGems.Parameters.Editor.Localization
{
    internal class CollectedLocalizationStrings : ICollectedLocalizationStrings
    {
        public IEnumerable<string> LocalizationKeys { get; }
        public IEnumerable<string> LocalizableScripts { get; }

        public CollectedLocalizationStrings(HashSet<string> localizationKeys, HashSet<string> localizableScripts)
        {
            LocalizationKeys = localizationKeys;
            LocalizableScripts = localizableScripts;
        }
    }
}
