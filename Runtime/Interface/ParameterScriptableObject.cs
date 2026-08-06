using System.Collections.Generic;
using PocketGems.Parameters.Validation;
using UnityEngine;

namespace PocketGems.Parameters.Interface
{
    public abstract class ParameterScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Returns any validation errors the scriptable object has.
        /// </summary>
        /// <returns></returns>
        public abstract ValidationError[] ValidationErrors();

        /// <summary>
        /// Populates the two data structures with any localized properties from this Scriptable Object.
        /// </summary>
        /// <param name="localizationKeys">localization keys</param>
        /// <param name="localizedScript">localized scripts</param>
        public abstract void CollectLocalizationStrings(HashSet<string> localizationKeys, HashSet<string> localizedScript);
#endif
    }
}
