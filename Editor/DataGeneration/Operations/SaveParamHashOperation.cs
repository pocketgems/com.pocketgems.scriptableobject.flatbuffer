using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
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
