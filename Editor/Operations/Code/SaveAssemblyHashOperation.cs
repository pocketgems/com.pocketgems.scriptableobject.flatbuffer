using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations.Code
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
