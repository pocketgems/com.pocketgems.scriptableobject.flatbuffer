#if (UNITY_EDITOR && ADDRESSABLE_PARAMS && UNITY_2021_3_OR_NEWER)
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.AssetLoader
{
    /// <summary>
    /// Editor subclass of the AddressablesParameterAssetLoader which handles loading the editor only iteration
    /// addressable assets and hot loading.
    /// </summary>
    [ExcludeFromCoverage]
    internal class EditorAddressablesParameterAssetLoader : AddressablesParameterAssetLoader, IParameterHotLoader
    {
        internal bool PromptEditorMessage = true;

        internal string EditorResourceDirectoryPath
        {
            get => _editorResourceDirectoryPath ?? ParameterConstants.GeneratedAsset.SubDirectory;
            set => _editorResourceDirectoryPath = value;
        }

        private string _editorResourceDirectoryPath;

        /// <inheritdoc/>
        public void HotLoadResource(string editorFilePath)
        {
            EditorAssetLoaderUtil.HotLoadResource(editorFilePath, ParameterDataLoader, ParameterManager);
        }

        /// <inheritdoc/>
        public IMutableParameterManager MutableParameterManager => ParameterManager;

        protected override void LoadData()
        {
            base.LoadData();
            /*
             * if using addressable bundles directly, do not load editor iteration files or prompt user of
             * missing files (allow normal user facing behavior)
             */
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var dataBuilder = settings.ActivePlayModeDataBuilder;
            var isUsingBundledAssets = !(dataBuilder is BuildScriptFastMode || dataBuilder is BuildScriptVirtualMode);
            if (isUsingBundledAssets)
                return;

            bool success = EditorAssetLoaderUtil.DirectlyLoadIterationFiles(this, EditorResourceDirectoryPath, PromptEditorMessage);
            Status = success ? ParameterAssetLoaderStatus.Loaded : ParameterAssetLoaderStatus.Failed;
        }
    }
}
#endif
