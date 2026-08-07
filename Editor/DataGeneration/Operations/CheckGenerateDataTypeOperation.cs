using System.IO;
using System.Reflection;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
#if ADDRESSABLE_PARAMS
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
#endif

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    internal class CheckGenerateDataTypeOperation : BasicOperation<IDataOperationContext>
    {
        /// <summary>
        /// Check the hash for the parameter interface against all generated hashes to ensure they match.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>true if the generated code is compatible with the current interface.</returns>
        private static bool IsGeneratedCodeCompatible(IDataOperationContext context)
        {
            var expectedHash = context.InterfaceAssemblyHash;
            return expectedHash == context.InterfaceHash.AssemblyInfoHash &&
                   expectedHash == context.InterfaceHash.AssemblyInfoEditorHash &&
                   expectedHash == context.InterfaceHash.GeneratedDataLoaderHash;
        }

        /// <summary>
        /// Check if all of the data needs to be regenerated instead of only the changed/new ones.
        /// </summary>
        /// <param name="context">the data context</param>
        /// <returns>true if full generation is required</returns>
        private static bool ShouldGenerateData(IDataOperationContext context)
        {
            var assetDirectory = context.GeneratedAssetDirectory;
            if (!Directory.Exists(assetDirectory))
                return true;

            var resourceFilePath = Path.Combine(assetDirectory, context.GeneratedAssetFileName);
            if (!File.Exists(resourceFilePath))
                return true;

            var hash = context.InterfaceHash.GeneratedDataHash;
            var expectedHash = context.InterfaceAssemblyHash;
            if (hash != expectedHash)
            {
                ParameterDebug.LogVerbose($"Detected old data hash [{hash}] - expected hash [{expectedHash}]");
                return true;
            }

            return false;
        }

        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            if (!IsGeneratedCodeCompatible(context))
            {
                Error("The generated code isn't up to date.  Run the code generation again.");
                return;
            }

            if (context.GenerateDataType == GenerateDataType.All)
                return;

            if (ShouldGenerateData(context))
            {
                ParameterDebug.LogVerbose($"Switching from {context.GenerateDataType} to {GenerateDataType.All}.");
                context.GenerateDataType = GenerateDataType.All;
                return;
            }

            if (context.GenerateDataType == GenerateDataType.IfNeeded)
            {
                ParameterDebug.LogVerbose($"Exit early no data generation needed.");
                ShortCircuit();
                return;
            }

#if ADDRESSABLE_PARAMS
            // If we're using remote bundles, the whole combined file must be regenerated so that it can be
            // uploaded to addressables - the iteration files a diff run produces are editor-only. A CSV diff
            // can't just switch to All here (the CSV edits must sync into the Scriptable Objects first), so
            // let it proceed and queue a full regeneration to rebuild the combined file afterward.
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;
            var editorDataBuilder = settings.ActivePlayModeDataBuilder;
#if ADDRESSABLES_2_0_0_OR_NEWER
            // BuildScriptVirtualMode ("Simulate Groups (advanced)") was removed in Addressables 2.0.
            bool isUsingRemoteBundles = !(editorDataBuilder is BuildScriptFastMode);
#else
            bool isUsingRemoteBundles =
                !(editorDataBuilder is BuildScriptFastMode || editorDataBuilder is BuildScriptVirtualMode);
#endif
            if (isUsingRemoteBundles)
            {
                if (context.GenerateDataType == GenerateDataType.CSVDiff)
                {
                    ParameterDebug.LogVerbose("Remote addressables in use - queueing a full regeneration after this CSV diff run.");
                    context.GenerateAllAgain = true;
                }
                else
                {
                    ParameterDebug.LogVerbose($"Switching from {context.GenerateDataType} to {GenerateDataType.All}.");
                    context.GenerateDataType = GenerateDataType.All;
                }
            }
#endif
        }
    }
}
