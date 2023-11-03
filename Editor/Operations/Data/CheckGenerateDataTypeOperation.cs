using System.IO;
using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Data
{
    internal class CheckGenerateDataTypeOperation : BasicOperation<IDataOperationContext>
    {
        private static bool ShouldGenerate(IDataOperationContext context)
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

            if (context.GenerateDataType == GenerateDataType.All)
                return;

            if (ShouldGenerate(context))
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
