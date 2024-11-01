using System.Collections.Generic;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.DataGeneration.LocalCSV.Editor;

namespace PocketGems.Parameters.DataGeneration.Operation.Editor
{
    public class DataOperationContext : CommonOperationContext, IDataOperationContext
    {
        public DataOperationContext()
        {
            GeneratedFilePaths = new List<string>();
            InfoCSVFileCache = new InfoCSVFileCache(GeneratedLocalCSVDirectory, true);
            StructCSVFileCache = new StructCSVFileCache(GeneratedLocalCSVDirectory, true);
            ScriptableObjectMetadatas = new Dictionary<IParameterInfo, List<IScriptableObjectMetadata>>();
        }

        public GenerateDataType GenerateDataType { get; set; }
        public bool GenerateAllAgain { get; set; }
        public List<string> ModifiedCSVPaths { get; set; }
        public List<string> ModifiedScriptableObjectPaths { get; set; }
        public IInfoCSVFileCache InfoCSVFileCache { get; }
        public IStructCSVFileCache StructCSVFileCache { get; }

        public string GeneratedCodeAssemblyName => EditorParameterConstants.CodeGeneration.AssemblyName;
        public string GeneratedCodeEditorAssemblyName => EditorParameterConstants.CodeGeneration.EditorAssemblyName;

        public string GeneratedLocalCSVDirectory => EditorParameterConstants.CSV.Dir;
        public string GeneratedAssetDirectory => ParameterConstants.GeneratedAsset.SubDirectory;
        public string GeneratedAssetFileName => EditorParameterConstants.GeneratedAsset.FileName;
#if ADDRESSABLE_PARAMS
        public string GeneratedAssetGuid => EditorParameterConstants.Addressables.HardCodedGuid;
        public string GeneratedAddressableGroup => EditorParameterConstants.Addressables.GroupName;
        public string GeneratedAddressableAddress => ParameterConstants.Addressables.GeneratedAssetAddress;
#endif
        public Dictionary<IParameterInfo, List<IScriptableObjectMetadata>> ScriptableObjectMetadatas { get; }
        public List<string> GeneratedFilePaths { get; }
    }
}
