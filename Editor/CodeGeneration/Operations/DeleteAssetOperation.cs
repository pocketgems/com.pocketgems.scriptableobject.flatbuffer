using System.IO;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEditor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    /// <summary>
    /// Deletes the specified asset from the project.
    /// </summary>
    internal class DeleteAssetOperation : BasicOperation<ICodeOperationContext>
    {
        private readonly string _filePath;

        /// <summary>
        /// Operation to remove temporary file if it exists.
        /// </summary>
        /// <param name="filePath">path to file</param>
        public DeleteAssetOperation(string filePath)
        {
            _filePath = filePath;
        }

        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);
            if (string.IsNullOrWhiteSpace(_filePath))
                return;

            var path = NamingUtil.RelativePath(_filePath);
            if (!File.Exists(path))
                return;

            if (!AssetDatabase.DeleteAsset(path))
                File.Delete(path);

            if (File.Exists(path))
                Error($"Unable to delete {path}");

            ParameterDebug.LogVerbose($"Deleted {path}");
        }
    }
}
