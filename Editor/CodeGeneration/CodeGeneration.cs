using System.Collections.Generic;
using PocketGems.Parameters.CodeGeneration.Operation.Editor;
using PocketGems.Parameters.CodeGeneration.Operations.Editor;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Operation.Editor;
using PocketGems.Parameters.Common.Operations.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.CodeGeneration.Editor
{
    [ExcludeFromCoverage]
    internal static class CodeGeneration
    {
        public static OperationExecutor<ICodeOperationContext> Generate(GenerateCodeType generateCodeType)
        {
            var executor = new OperationExecutor<ICodeOperationContext>();
            var context = new CodeOperationContext();
            context.GenerateCodeType = generateCodeType;
            executor.ExecuteOperations(new List<IParameterOperation<ICodeOperationContext>>()
            {
                // populate context with assembly information & types
                new ParseInterfaceAssemblyOperation<ICodeOperationContext>(),
                // validate defined interfaces, enums & properties in the interface assembly
                new ValidateTypesOperation(),
                // check if the operation should be canceled due to up to date generated code
                new CheckGenerateCodeTypeOperation(),
                // build the ParameterSchema.fbs file to be consumed by the flatbuffer flatc binary.
                new BuildSchemaOperation(),
                // generate the big files
                new GenerateCodeOperation(),
                // generate the C# structs with the schema defined in ParameterSchema.fbs
                new GenerateFlatBufferStructsOperation(),
                // create the FlatBuffer & ScriptableObject implementation files.
                new GenerateImplementationFilesOperation(),
                // create validation files if needed
                new GenerateValidatorFilesOperation(),
                // save the latest hash value to the assembly files
                new SaveAssemblyHashOperation(),
                // Delete the intermediate Schema.fbs file.
                new DeleteAssetOperation(ParameterPrefs.DoNotDeleteSchemaFile
                    ? null
                    : EditorParameterConstants.CodeGeneration.SchemaFilePath),
            }, context);
            return executor;
        }
    }
}
