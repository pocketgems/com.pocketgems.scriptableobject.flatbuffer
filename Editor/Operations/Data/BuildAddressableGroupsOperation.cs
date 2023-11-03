#if ADDRESSABLE_PARAMS
using System.Collections.Generic;
using System.IO;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Operations
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

            // check addressable is set up
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Error("Addressable Settings is not set up. Addressable settings cannot be found.");
                return;
            }

            var parameterGuid = context.GeneratedAssetGuid;

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
            }

            // create addressable group if needed
            var groupName = context.GeneratedAddressableGroup;
            var addressableAssetGroup = settings.FindGroup(groupName);
            if (addressableAssetGroup == null)
            {
                var defaultGroup = settings.DefaultGroup;
                addressableAssetGroup = settings.CreateGroup(groupName, false, false, true, defaultGroup.Schemas);
            }

            // add to addressable group
            var parameterEntry = settings.FindAssetEntry(parameterGuid);
            if (parameterEntry == null || parameterEntry.parentGroup != addressableAssetGroup)
                parameterEntry = settings.CreateOrMoveEntry(parameterGuid, addressableAssetGroup, true);
            parameterEntry.address = context.GeneratedAddressableAddress;

            // save addressables changes
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
