using UnityEngine;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public interface IScriptableObjectMetadata
    {
        string GUID { get; }
        string FilePath { get; }
        ScriptableObject ScriptableObject { get; }

        public void Rename(string newName);
    }
}
