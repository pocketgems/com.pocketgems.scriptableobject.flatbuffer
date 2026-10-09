using System.Collections.Generic;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.DataGeneration.Util.Editor;

namespace PocketGems.Parameters.DataGeneration.Operations.Editor
{
    /// <summary>
    /// Saves the guids of the loaded Scriptable Objects so deleted parameters can be recognized later.
    /// </summary>
    internal class SaveParamGuidsOperation : BasicOperation<IDataOperationContext>
    {
        public override void Execute(IDataOperationContext context)
        {
            base.Execute(context);

            var guids = new List<string>();
            foreach (var metadatas in context.ScriptableObjectMetadatas.Values)
                for (int i = 0; i < metadatas.Count; i++)
                    guids.Add(metadatas[i].GUID);

            if (context.GenerateDataType == GenerateDataType.All)
                ParameterGuidCache.Replace(guids);
            else
                ParameterGuidCache.Add(guids);
        }
    }
}
