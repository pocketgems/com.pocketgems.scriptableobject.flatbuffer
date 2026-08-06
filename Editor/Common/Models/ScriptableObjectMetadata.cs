using System.IO;
using PocketGems.Parameters.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public class ScriptableObjectMetadata : IScriptableObjectMetadata
    {
        public ScriptableObjectMetadata(string guid, string filePath, ParameterScriptableObject scriptableObject)
        {
            GUID = guid;
            FilePath = filePath;
            ScriptableObject = scriptableObject;
        }

        public string GUID { get; }
        public string FilePath { get; private set; }
        public ParameterScriptableObject ScriptableObject { get; private set; }

        [ExcludeFromCoverage]
        public void Rename(string newName)
        {
            AssetDatabase.RenameAsset(FilePath, newName);
            FilePath = Path.Combine(Path.GetDirectoryName(FilePath), $"{newName}{Path.GetExtension(FilePath)}");
            ScriptableObject = AssetDatabase.LoadAssetAtPath<ParameterScriptableObject>(FilePath);
        }
    }
}
