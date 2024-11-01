#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Parameter loader that loads assets directly in editor without using Resources or Addressables
    /// </summary>
    internal class EditorDirectFileParameterAssetLoader : IParameterHotLoader
    {
        /// <inheritdoc/>
        public ParameterAssetLoaderStatus Status { get; private set; }

        private IMutableParameterManager ParameterManager;
        private IParameterDataLoader ParameterDataLoader;
        private readonly string _fileDirectoryPath;

        public EditorDirectFileParameterAssetLoader(string fileDirectoryPath = null)
        {
            Status = ParameterAssetLoaderStatus.NotStarted;
            _fileDirectoryPath = fileDirectoryPath ?? ParameterConstants.GeneratedAsset.SubDirectory;
        }

        /// <inheritdoc/>
        public void HotLoadResource(string editorFilePath)
        {
            EditorAssetLoaderUtil.HotLoadResource(editorFilePath, ParameterDataLoader, ParameterManager);
        }

        /// <inheritdoc/>
        public IMutableParameterManager MutableParameterManager => ParameterManager;

        /// <inheritdoc/>
        public void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader)
        {
            Status = ParameterAssetLoaderStatus.Loading;
            ParameterManager = parameterManager;
            ParameterDataLoader = parameterDataLoader;

            /*
             * In the editor load the root parameter file first, then all others.
             *
             * During iteration in the editor, the BuildDataBufferOperation.cs regenerates .byte files for
             * parameter files that had changed data to optimize iteration speed.  This is done in
             * BuildDataBufferOperation.cs.
             */
            bool success = EditorAssetLoaderUtil.DirectlyLoadMainFile(this, _fileDirectoryPath);
            success &= EditorAssetLoaderUtil.DirectlyLoadIterationFiles(this, _fileDirectoryPath, false);
            Status = success ? ParameterAssetLoaderStatus.Loaded : ParameterAssetLoaderStatus.Failed;
        }
    }
}
#endif
