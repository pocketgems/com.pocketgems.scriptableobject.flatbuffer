using System.Collections.Generic;
using PocketGems.Parameters.Editor.LocalCSV;
using PocketGems.Parameters.Models;

namespace PocketGems.Parameters.Editor.Operation
{
    public interface IDataOperationContext : ICommonOperationContext
    {
        // generation type used by the generate data operation
        GenerateDataType GenerateDataType { get; set; }
        bool GenerateAllAgain { get; set; }
        List<string> ModifiedCSVPaths { get; set; }
        List<string> ModifiedScriptableObjectPaths { get; set; }

        // csv file caches
        IInfoCSVFileCache InfoCSVFileCache { get; }
        IStructCSVFileCache StructCSVFileCache { get; }

        // assembly names
        string GeneratedCodeAssemblyName { get; }
        string GeneratedCodeEditorAssemblyName { get; }

        // generated assets
        string GeneratedLocalCSVDirectory { get; }

        string GeneratedAssetDirectory { get; }
        string GeneratedAssetFileName { get; }
#if ADDRESSABLE_PARAMS
        string GeneratedAssetGuid { get; }
        string GeneratedAddressableGroup { get; }
        string GeneratedAddressableAddress { get; }
#endif

        /// <summary>
        /// Scriptable Objects that are being modified (modified by the user or needs to be updated from CSVs).
        /// </summary>
        Dictionary<IParameterInfo, List<IScriptableObjectMetadata>> ScriptableObjectMetadatas { get; }

        /// <summary>
        /// Asset files that were generated through this data generation
        /// </summary>
        List<string> GeneratedFilePaths { get; }
    }
}
