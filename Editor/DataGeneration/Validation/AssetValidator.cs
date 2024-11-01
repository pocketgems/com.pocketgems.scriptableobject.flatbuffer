using System.Collections.Generic;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Validation;
#if ADDRESSABLE_PARAMS
using UnityEditor.AddressableAssets;
#endif
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Validation.Editor
{
    [ExcludeFromCoverage]
    public static class AssetValidator
    {
        public static IReadOnlyList<ValidationError> ValidateScriptableObjects(
            Dictionary<IParameterInfo, List<IScriptableObjectMetadata>> metadatas)
        {
#if ADDRESSABLE_PARAMS
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            List<ValidationError> errors = new List<ValidationError>();
            HashSet<string> addressableGuids = new HashSet<string>();
            // it is faster to construct this cache up front
            // it's more expensive to call settings.FindAssetEntry() which iterates through all groups every call.
            var groups = settings.groups;
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (group == null)
                    continue;
                foreach (var entry in group.entries)
                {
                    if (entry == null)
                        continue;
                    addressableGuids.Add(entry.guid);
                }
            }
            foreach (var kvp in metadatas)
            {
                var parameterInterface = kvp.Key;
                var scriptableObjectMetadatas = kvp.Value;
                for (int i = 0; i < scriptableObjectMetadatas.Count; i++)
                {
                    var metadata = scriptableObjectMetadatas[i];
                    if (!addressableGuids.Contains(metadata.GUID))
                        continue;

                    var error = new ValidationError(parameterInterface.Type,
                        metadata.ScriptableObject.name, null,
                        "parameter scriptable object should not be an addressable asset");
                    errors.Add(error);
                }
            }
            return errors;
#else
            return null;
#endif
        }
    }
}
