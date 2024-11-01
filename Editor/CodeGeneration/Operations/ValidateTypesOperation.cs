using System.Collections.Generic;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Operations.Editor;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    internal class ValidateTypesOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            var parameterEnums = context.ParameterEnums;
            for (int i = 0; i < parameterEnums.Count; i++)
            {
                if (!parameterEnums[i].Validate(out List<string> errors))
                    foreach (var error in errors)
                        Error(error);
            }

            for (int i = 0; i < context.ParameterStructs.Count; i++)
                Validate(context.ParameterStructs[i]);

            for (int i = 0; i < context.ParameterInfos.Count; i++)
                Validate(context.ParameterInfos[i]);
        }

        private void Validate(IParameterInterface parameterInterface)
        {
            if (!parameterInterface.Validate(out IReadOnlyList<string> errors))
                foreach (var error in errors)
                    Error(error);
        }
    }
}
