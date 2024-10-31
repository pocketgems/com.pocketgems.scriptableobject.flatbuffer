using System.IO;
using System.Reflection;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

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
            }
        }
    }
}
