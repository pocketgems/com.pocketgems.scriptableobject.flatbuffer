#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.Interface;
using UnityEditor;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace PocketGems.Parameters.LocalCSV
{
    /// <summary>
    /// Cache to cache all of the ParameterScriptable objects by name & guid.
    ///
    /// The call to AssetDatabase.FindAssets is slow and doing a one time call in this cache
    /// is significantly faster than individually for finding individual assets.
    /// </summary>
    [ExcludeFromCoverage]
    internal static class ScriptableObjectLookupCache
    {
        public static bool Enabled
        {
            get => s_enabled;
            set
            {
                var oldValue = s_enabled;
                if (oldValue == value)
                    return;
                s_enabled = value;
                if (s_enabled)
                    RebuildBuildCache();
                else
                    s_cache.Clear();
            }
        }

        public static List<(string, string)> LookUp(string name)
        {
            if (!Enabled)
            {
                Debug.LogError("Cannot LookUp in Cache, it is disabled.");
                return null;
            }

            if (s_cache.TryGetValue(name, out List<(string, string)> list))
                return list;
            return null;
        }

        public static void RebuildBuildCache()
        {
            if (!Enabled)
            {
                Debug.LogError("Cannot Build Cache, it is disabled");
                return;
            }

            s_cache.Clear();
            var searchText = $"t:{nameof(ParameterScriptableObject)}";
            var guids = AssetDatabase.FindAssets(searchText);
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var name = Path.GetFileNameWithoutExtension(path);
                if (!s_cache.TryGetValue(name, out List<(string, string)> list))
                {
                    list = new List<(string, string)>();
                    s_cache[name] = list;
                }
                list.Add((guids[i], path));
            }
        }

        private static bool s_enabled;
        private static readonly Dictionary<string, List<(string, string)>> s_cache = new Dictionary<string, List<(string, string)>>();
    }
}
#endif
