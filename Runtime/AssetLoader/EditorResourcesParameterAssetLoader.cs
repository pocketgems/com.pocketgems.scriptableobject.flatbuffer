#if UNITY_EDITOR

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Asset loader used strictly in editor.
    /// </summary>
    internal class EditorResourcesParameterAssetLoader : ResourcesParameterAssetLoader, IParameterHotLoader
    {
        internal bool PromptEditorMessage = true;
        internal string EditorResourceDirectoryPath
        {
            get => _editorResourceDirectoryPath ?? ParameterConstants.GeneratedAsset.SubDirectory;
            set => _editorResourceDirectoryPath = value;
        }

        private string _editorResourceDirectoryPath;

        public EditorResourcesParameterAssetLoader(string resourceDirectoryPath = ParameterConstants.GeneratedResourceDirectory) : base(resourceDirectoryPath)
        {
        }

        /// <inheritdoc/>
        public void HotLoadResource(string editorFilePath)
        {
            EditorAssetLoaderUtil.HotLoadResource(editorFilePath, ParameterDataLoader, ParameterManager);
        }

        /// <inheritdoc/>
        public IMutableParameterManager MutableParameterManager => ParameterManager;

        /// <summary>
        /// Loads data through the ResourcesParameterAssetLoader super class, provides an additional debug prompt,
        /// and loads iterative flat buffer files for faster editor iteration.
        /// </summary>
        /// <param name="parameterManager">Parameter manager to load parameter data into.</param>
        /// <param name="parameterDataLoader">The parameter data loader used to pass asset byte data to.</param>
        public override void LoadData(IMutableParameterManager parameterManager, IParameterDataLoader parameterDataLoader)
        {
            base.LoadData(parameterManager, parameterDataLoader);
            bool success = EditorAssetLoaderUtil.DirectlyLoadIterationFiles(this, EditorResourceDirectoryPath, PromptEditorMessage);
            Status = success ? ParameterAssetLoaderStatus.Loaded : ParameterAssetLoaderStatus.Failed;
        }
    }
}
#endif
