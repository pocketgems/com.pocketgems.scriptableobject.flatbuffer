using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Operations.Code
{
    /// <summary>
    /// Generate the implementation files that implement the interfaces
    /// </summary>
    public class GenerateValidatorFilesOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            var scriptableObjectDir = context.GeneratedCodeValidatorsDir;

            bool generated = false;
            for (int i = 0; i < context.ParameterInfos.Count; i++)
            {
                var parameterInterface = context.ParameterInfos[i];
                if (CodeGenerator.AttemptGenerateValidationFile(parameterInterface, scriptableObjectDir))
                    generated = true;
            }

            for (int i = 0; i < context.ParameterStructs.Count; i++)
            {
                var parameterInterface = context.ParameterStructs[i];
                if (CodeGenerator.AttemptGenerateValidationFile(parameterInterface, scriptableObjectDir))
                    generated = true;
            }

            if (generated)
                ParameterDebug.Log($"Generated validation file(s) in {scriptableObjectDir}");
        }
    }
}
