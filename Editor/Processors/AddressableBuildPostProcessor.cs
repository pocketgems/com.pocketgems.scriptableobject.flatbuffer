#if ADDRESSABLE_PARAMS
using PocketGems.Parameters.AssetLoader;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Processors.Editor
{
    [ExcludeFromCoverage]
    public static class AddressableBuildPostProcessor
    {
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            /*
             * Check addressable bundle builds after completion to warn the user of missed parameters.
             *
             * During editor iteration, iteration files are created and not included as part of the addressable group.
             * A full parameter regeneration needs to be completed prior to addressable asset bundle building.
             *
             * Due to a lack of an pre-asset build hook in the Addressable package, this check is done in this
             * post build hook.
             */
            BuildScript.buildCompleted += result =>
            {
                if (!string.IsNullOrEmpty(result.Error))
                    return;

                // only check on bundle builds (not player builds)
                if (!(result is AddressablesPlayerBuildResult))
                    return;

                var iterationFiles = EditorAssetLoaderUtil.GetGeneratedParameterFiles(ParameterConstants.GeneratedAsset.SubDirectory, false);
                if (iterationFiles.Count == 0)
                    return;

                bool shouldRegenerate = EditorUtility.DisplayDialog("Missing Parameters",
                    "Parameter addressable group is missing updated data.  Regenerate the parameter file and rebuild " +
                    "Addressables again.",
                    "Regenerate", "Cancel");
                if (!shouldRegenerate)
                    return;

                var success = EditorParameterDataManager.GenerateData(GenerateDataType.All, out _);
                string message;
                if (success)
                    message = "Success: please regenerate addressable bundles again.";
                else
                    message = "Fail: please see console for parameter errors.";
                EditorUtility.DisplayDialog("Parameters Generation", message, "Okay");
            };
        }
    }
}
#endif
