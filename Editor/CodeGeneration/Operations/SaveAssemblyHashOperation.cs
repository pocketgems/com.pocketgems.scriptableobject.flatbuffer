using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    internal class SaveAssemblyHashOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            context.InterfaceHash.AssemblyInfoHash = context.InterfaceAssemblyHash;
            context.InterfaceHash.AssemblyInfoEditorHash = context.InterfaceAssemblyHash;
        }
    }
}
