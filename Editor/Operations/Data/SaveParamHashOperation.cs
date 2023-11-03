using PocketGems.Parameters.Editor.Operation;

namespace PocketGems.Parameters.Operations.Data
{
    internal class SaveParamHashOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            context.InterfaceHash.GeneratedDataHash = context.InterfaceAssemblyHash;
        }
    }
}
