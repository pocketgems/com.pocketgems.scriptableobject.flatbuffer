using PocketGems.Parameters.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Util.Editor
{
    [ExcludeFromCoverage]
    public static class ScriptableObjectUtil
    {
        /// <summary>
        /// Returns all scriptable objects in the parameters Scriptable Objects folder.
        /// </summary>
        /// <returns>array of guids</returns>
        public static string[] FindAllParameterScriptableObjects(string className = null)
        {
            var searchText = $"t:{className ?? nameof(ParameterScriptableObject)}";
            return AssetDatabase.FindAssets(searchText);
        }

        /// <summary>
        /// Loads & re saves Scriptable Objects so that any FormerlySerializedAs fields are updated.
        /// </summary>
        public static void ReSaveAllInfoScriptableObjects()
        {
            string[] guids = FindAllParameterScriptableObjects();
            foreach (var guid in guids)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);
                if (!filePath.EndsWith(".asset"))
                    continue;
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(filePath);
                if (!(asset is IBaseInfo))
                    continue;
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
