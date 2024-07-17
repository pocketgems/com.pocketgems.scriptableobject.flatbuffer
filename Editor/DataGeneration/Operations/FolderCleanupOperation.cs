using System.IO;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using UnityEditor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    /// <summary>
    /// Operation to clean up relics from from no longer utilized asset creation.
    /// </summary>
    internal class FolderCleanupOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            // remove the old path for the resources folder
            string[] legacyPathDirs = { "Assets", "Parameters", "Resources" };
            var legacyResourcesPath = Path.Combine(legacyPathDirs);
            if (Directory.Exists(legacyResourcesPath))
                AssetDatabase.DeleteAsset(legacyResourcesPath);

            // delete the Resource folder if building Addressable (or vice versa)
            string[] rootPathDir = { "Assets", "Parameters", "GeneratedAssets" };
            var rootPath = Path.Combine(rootPathDir);
            if (Directory.Exists(rootPath))
            {
                var directories = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var directory in directories)
                    if (!context.GeneratedAssetDirectory.StartsWith(directory))
                        AssetDatabase.DeleteAsset(directory);
            }
        }
    }
}
