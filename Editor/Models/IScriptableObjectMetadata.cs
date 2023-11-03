using UnityEngine;

namespace PocketGems.Parameters.Models
{
    public interface IScriptableObjectMetadata
    {
        string GUID { get; }
        string FilePath { get; }
        ScriptableObject ScriptableObject { get; }

        public void Rename(string newName);
    }
}
