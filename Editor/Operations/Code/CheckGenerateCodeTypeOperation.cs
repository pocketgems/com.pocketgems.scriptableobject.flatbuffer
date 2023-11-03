using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Code
{
    internal class CheckGenerateCodeTypeOperation : BasicOperation<ICodeOperationContext>
    {
        private static bool ShouldGenerate(ICodeOperationContext context)
        {
            bool generate = false;

            var hash = context.InterfaceHash.AssemblyInfoHash;
            var expectedHash = context.InterfaceAssemblyHash;
            if (hash != expectedHash)
            {
                ParameterDebug.LogVerbose(
                    $"Detected old hash [{hash}] in AssemblyInfo - expected hash [{expectedHash}]");
                generate = true;
            }

            hash = context.InterfaceHash.AssemblyInfoEditorHash;
            if (hash != expectedHash)
            {
                ParameterDebug.LogVerbose(
                    $"Detected old hash [{hash}] in Editor AssemblyInfo - expected hash [{expectedHash}]");
                generate = true;
            }

            return generate;
        }

        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            if (context.GenerateCodeType == GenerateCodeType.Generate)
                return;

            if (ShouldGenerate(context))
            {
                ParameterDebug.LogVerbose($"Switching from {context.GenerateCodeType} to {GenerateCodeType.Generate}.");
                context.GenerateCodeType = GenerateCodeType.Generate;
                return;
            }

            if (context.GenerateCodeType == GenerateCodeType.IfNeeded)
            {
                ParameterDebug.LogVerbose($"Exit early no new code to generate.");
                ShortCircuit();
            }
        }
    }
}
