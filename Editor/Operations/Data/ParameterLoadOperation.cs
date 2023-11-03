using PocketGems.Parameters.AssetLoader;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;
using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Operations.Data
{
    /// <summary>
    /// It rebuilds the FlatBuffer data file and updates the ParameterManager that is currently running with latest
    /// assets.
    /// </summary>
    [ExcludeFromCoverage]
    internal class ParameterLoadOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            LoadEditorParams(context);
            HotloadPlayMode(context);
        }

        private void LoadEditorParams(IDataOperationContext context)
        {
            if (context.GenerateDataType == GenerateDataType.All
                || EditorParams.ParameterManager == null)
            {
                EditorParams.Init();
                return;
            }

            if (context.GenerateDataType == GenerateDataType.ScriptableObjectDiff ||
                context.GenerateDataType == GenerateDataType.CSVDiff)
            {
                var filePaths = context.GeneratedFilePaths;
                for (int i = 0; i < filePaths.Count; i++)
                    EditorParams.HotLoader.HotLoadResource(filePaths[i]);
            }
        }

        private void HotloadPlayMode(IDataOperationContext context)
        {

            if (!EditorApplication.isPlaying || !ParameterPrefs.PlayModeHotLoading)
                return;

            var assetLoader = ParameterAssetLoaderProvider.RunningHotLoader;
            if (assetLoader == null)
            {
                ParameterDebug.Log(
                    $"Cannot hot load parameters, can't find instance of: {typeof(IParameterHotLoader)}.");
                return;
            }

            if (context.GenerateDataType == GenerateDataType.All)
                assetLoader.MutableParameterManager.RemoveAll();
            var filePaths = context.GeneratedFilePaths;
            for (int i = 0; i < filePaths.Count; i++)
            {
                assetLoader.HotLoadResource(filePaths[i]);
                ParameterDebug.Log($"Hot loaded parameter file: {filePaths[i]}.");
            }
        }
    }
}
