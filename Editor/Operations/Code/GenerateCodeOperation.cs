using PocketGems.Parameters.Editor.Operation;
using PocketGems.Parameters.Util;
using UnityEditor;

namespace PocketGems.Parameters.Operations.Code
{
    /// <summary>
    /// Generating various code files
    /// </summary>
    internal class GenerateCodeOperation : BasicOperation<ICodeOperationContext>
    {
        public override void Execute(ICodeOperationContext context)
        {
            base.Execute(context);

            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building ParamsSetup File", 100);
            CodeGenerator.GenerateParamsSetup(context.GeneratedCodeDir);

            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building DataLoader File", 100);
            CodeGenerator.GenerateDataLoader(context.InterfaceAssemblyHash, context.ParameterInfos,
                context.ParameterStructs, context.GeneratedCodeDir);

            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building Validation File", 100);
            CodeGenerator.GenerateParamsValidation(context.ParameterInfos, context.GeneratedCodeDir);

            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building FlatBufferBuilder File", 100);
            CodeGenerator.GenerateFlatBufferBuilder(context.ParameterInfos, context.ParameterStructs,
            context.GeneratedCodeFlatBufferBuilderDir);

            EditorUtility.DisplayProgressBar("Generating Parameter Code", "Building CSV Bridge File", 100);
            CodeGenerator.GenerateCSVBridge(context.ParameterInfos, context.ParameterStructs, context.GeneratedCodeCSVBridgeDir);

            EditorUtility.ClearProgressBar();
        }
    }
}
