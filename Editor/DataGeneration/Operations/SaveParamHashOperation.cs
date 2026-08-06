using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    internal class SaveParamHashOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            // A full regeneration is queued and will save the hash after it completes. Avoid marking the data
            // as current now - it is both wasted and could mask that the data is mid-regeneration.
            if (context.GenerateAllAgain)
                return;

            context.InterfaceHash.GeneratedDataHash = context.InterfaceAssemblyHash;
        }
    }
}
