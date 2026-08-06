using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public interface IScriptableObjectMetadata
    {
        string GUID { get; }
        string FilePath { get; }
        ParameterScriptableObject ScriptableObject { get; }

        public void Rename(string newName);
    }
}
