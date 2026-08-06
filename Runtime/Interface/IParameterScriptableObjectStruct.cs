using System.Collections.Generic;

namespace PocketGems.Parameters.Interface
{
    /// <summary>
    /// Interface for structs that are declared in Scriptable Objects.
    /// </summary>
    public interface IParameterScriptableObjectStruct
    {
        /// <summary>
        /// Populates the two data structures with any localized properties from this Struct.
        /// </summary>
        /// <param name="localizationKeys">localization keys</param>
        /// <param name="localizedScript">localized scripts</param>
        void CollectLocalizationStrings(HashSet<string> localizationKeys, HashSet<string> localizedScript);
    }
}
