#if ADDRESSABLE_PARAMS
using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    /// <summary>
    /// Creates the an addressable group for parameters and adds all assets as addressable assets to the group.
    /// </summary>
    [ExcludeFromCoverage]
    internal class BuildAddressableGroupsOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            // A full regeneration is queued and will rebuild + reconfigure the addressable group afterward.
            if (context.GenerateAllAgain)
                return;

            // check addressable is set up
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Error("Addressable Settings is not set up. Addressable settings cannot be found.");
                return;
            }

            var parameterGuid = context.GeneratedAssetGuid;

            // track whether we actually mutated any addressable state; if nothing changed we can skip
            // the expensive AssetDatabase.SaveAssets() (a full dirty-asset flush) at the end.  In diff
            // generation the combined data file isn't rewritten, so every check below is a no-op and
            // this operation was previously paying for a SaveAssets() that had nothing to persist.
            bool changed = false;

            // Set the guid for the parameter.byte file so that it's always the same.
            // This is so the source controlled addressable group reference to the asset isn't broken.
            var outputDirectory = context.GeneratedAssetDirectory;
            var outputFilename = context.GeneratedAssetFileName;
            var outputFilePath = Path.Combine(outputDirectory, outputFilename);
            if (AssetDatabase.AssetPathToGUID(outputFilePath) != parameterGuid)
            {
                var metaFilePath = outputFilePath + ".meta";
                var args = new Dictionary<string, object>
                {
                    { "guid", parameterGuid }
                };
                ScribanHelper.GenerateClass(EditorParameterConstants.Addressables.MetaTemplateFileName, metaFilePath, args);
                // re-import asset to update the guid
                AssetDatabase.ImportAsset(outputFilePath);
                if (AssetDatabase.AssetPathToGUID(outputFilePath) != parameterGuid)
                {
                    Error($"Unable to set GUID to [{parameterGuid}] for [{outputFilePath}]");
                    return;
                }
                ParameterDebug.LogVerbose($"Updated GUID to [{parameterGuid}] for {outputFilePath}");
                changed = true;
            }

            // create addressable group if needed
            var groupName = context.GeneratedAddressableGroup;
            var addressableAssetGroup = settings.FindGroup(groupName);
            if (addressableAssetGroup == null)
            {
                var defaultGroup = settings.DefaultGroup;
                addressableAssetGroup = settings.CreateGroup(groupName, false, false, true, defaultGroup.Schemas);
                changed = true;
            }

            // add to addressable group
            var parameterEntry = settings.FindAssetEntry(parameterGuid);
            if (parameterEntry == null || parameterEntry.parentGroup != addressableAssetGroup)
            {
                parameterEntry = settings.CreateOrMoveEntry(parameterGuid, addressableAssetGroup, true);
                changed = true;
            }
            if (parameterEntry.address != context.GeneratedAddressableAddress)
            {
                parameterEntry.address = context.GeneratedAddressableAddress;
                changed = true;
            }

            // save addressables changes (only when something was actually modified)
            if (changed)
                AssetDatabase.SaveAssets();
        }
    }
}
#endif
