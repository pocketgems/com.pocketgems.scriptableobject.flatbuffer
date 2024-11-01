using System.Collections.Generic;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.CodeGeneration.Util.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEditor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.CodeGeneration.Operations.Editor
{
    /// <summary>
    /// Uses the flatbuffer binary to generate the csharp structs for the provided schema file.
    /// </summary>
    [ExcludeFromCoverage]
    internal class GenerateFlatBufferStructsOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            var schemaFilePath = context.SchemaFilePath;
            var outputDir = context.GeneratedCodeFlatBufferStructsDir;
            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building Source Files", 100);
            var success = CodeGenerator.GenerateFlatBufferStructs(schemaFilePath, outputDir, out List<string> errors);
            if (success)
            {
                ParameterDebug.Log($"Generated source files in {outputDir}");
            }
            else
            {
                for (int i = 0; i < errors.Count; i++)
                    Error(errors[i]);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
